using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using Microsoft.AspNetCore.Mvc;
using OverLut.Models.DTOs;
using OverLut.Models.Repositories;
using System.Threading.Tasks;

namespace OverLut.Controllers
{
    public class ChatPageController : Controller
    {
        private readonly IChatRepository _chatRepository;
        public ChatPageController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        // Chunked upload endpoints
        [HttpPost]
        [Route("ChatPage/StartUpload")]
        public async Task<IActionResult> StartUpload([FromForm] string fileName, [FromForm] string contentType)
        {
            if (!Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userGuid)) return Unauthorized();
            var blobId = await _chatRepository.StartUploadAsync(fileName, contentType);
            if (blobId == null) return BadRequest("Failed to start upload");
            return Json(new { fileBlobId = blobId });
        }

        [HttpPost]
        [Route("ChatPage/UploadChunk")]
        public async Task<IActionResult> UploadChunk([FromForm] Guid fileBlobId, [FromForm] int sequenceNumber, [FromForm] IFormFile chunk)
        {
            if (!Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userGuid)) return Unauthorized();
            if (chunk == null) return BadRequest("No chunk");

            byte[] data;
            using (var ms = new MemoryStream())
            {
                await chunk.CopyToAsync(ms);
                data = ms.ToArray();
            }

            var ok = await _chatRepository.UploadChunkAsync(fileBlobId, sequenceNumber, data);
            if (!ok) return BadRequest("Failed to save chunk");
            return Ok();
        }

        [HttpPost]
        [Route("ChatPage/FinishUpload")]
        public async Task<IActionResult> FinishUpload([FromForm] Guid fileBlobId, [FromForm] Guid channelId, [FromForm] long fileSize, [FromForm] string fileName, [FromForm] string contentType)
        {
            if (!Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userGuid)) return Unauthorized();

            var attachment = await _chatRepository.FinishUploadAsync(fileBlobId, channelId, fileSize, userGuid, fileName, contentType);
            if (attachment == null) return BadRequest("Failed to finalize upload");

            return Json(new {
                attachmentId = attachment.AttachmentId,
                fileName = attachment.FileName,
                contentType = attachment.ContentType,
                fileBlobId = attachment.FileBlobId,
                messageId = attachment.MessageId
            });
        }
        // GET: ChatPageController
        [Authorize]
        public async Task<ActionResult> Index()
        {
            if (Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userGuid))
            {
                var lsChannel = await _chatRepository.GetAllChannelByUserIDAsync(userGuid);
                return View(lsChannel);
            }
            else
            {
                return Unauthorized();
            }
        }
        // GET: ChatPage/logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Login");
        }

        // GET: /ChatPage/GetMessage/
        [HttpGet]
        [Route("ChatPage/GetMessage/{id}")]
        public async Task<IActionResult> GetMessage(Guid id)
        {
            var msg = await _chatRepository.GetMessagesByChannelIDAsync(id);
            return Json(msg);
        }

        [HttpPost]
        [Route("ChatPage/SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (request == null) return BadRequest();

            if (Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userGuid))
            {
                var messageDto = new MessageDTO
                {
                    UserId = userGuid,
                    Content = request.Content,
                    ChannelId = request.ChannelId,
                    MessageType = 0,
                };

                var success = await _chatRepository.SendMessageAsync(messageDto);

                if (success)
                {
                    // return saved message back to the caller (client can use it as confirmation)
                    return Ok(messageDto);
                }
                else
                {
                    return BadRequest("Failed to send message (maybe not a channel member).");
                }
            }
            return Unauthorized();
        }

        public class SendMessageRequest
        {
            public Guid ChannelId { get; set; }
            public string Content { get; set; }
        }

        // POST: /ChatPage/UploadAttachment
        [HttpPost]
        [Route("ChatPage/UploadAttachment")]
        public async Task<IActionResult> UploadAttachment(IFormFile file, Guid channelId)
        {
            if (file == null) return BadRequest("No file provided");

            if (!Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userGuid))
            {
                return Unauthorized();
            }

            var attach = await _chatRepository.UploadAttachmentAsync(file, channelId, userGuid);
            if (attach == null) return BadRequest("Failed to save attachment");

            // return attachment metadata and the message so clients can display immediately (camelCase)
            return Json(new {
                attachmentId = attach.AttachmentId,
                fileName = attach.FileName,
                contentType = attach.ContentType,
                fileBlobId = attach.FileBlobId,
                messageId = attach.MessageId
            });
        }

        // GET: /ChatPage/DownloadAttachment/{id}
        [HttpGet]
        [Route("ChatPage/DownloadAttachment/{id}")]
        public async Task<IActionResult> DownloadAttachment(Guid id)
        {
            var att = await _chatRepository.GetAttachmentAsync(id);
            if (att == null) return NotFound();

            var blob = await _chatRepository.DownloadAttachmentAsync(id);
            if (blob == null) return NotFound();

            return File(blob.Value.data, blob.Value.contentType, blob.Value.fileName);
        }


    }
}
