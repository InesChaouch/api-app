using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController(IUnitOfWork unitOfWork, IMapper mapper): BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<MemberDto>> CreateMessage(CreateMessageDto createMessageDto){
            
            var username = User.GetUsername();
            if (username == createMessageDto.RecipientUsername.ToLower()) 
                return BadRequest("you cannot message yoursef");
            
            var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            if(recipient==null || sender==null) 
                return BadRequest("Cannot send message at this time");
            
            var message = new Message{
                RecipientUsername = createMessageDto.RecipientUsername,
                Recipient = recipient,
                SenderUsername = username,
                Sender = sender,
                Content = createMessageDto.Content
            };
            unitOfWork.MessageRepository.AddMessage(message);
            if (await unitOfWork.Complete()) return Ok(mapper.Map<MessageDto>(message));
            return BadRequest("Failed to save message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams){
            messageParams.Username= User.GetUsername();

            var messages = await unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages);

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username) {
            
            var currentUsername = User.GetUsername();

            return Ok(await unitOfWork.MessageRepository.GetMessagesThread(currentUsername,username));
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id){
            var username = User.GetUsername();

            var message = await unitOfWork.MessageRepository.GetMessage(id);

            if(message == null) return BadRequest("Cannot delete this message");

            if(message.SenderUsername!= username && message.RecipientUsername != username) 
            return Forbid();

            if(message.SenderUsername == username) message.SenderDeleted = true;
            if(message.SenderUsername == username) message.RecipientDeleted = true;

            if(message is {SenderDeleted: true, RecipientDeleted: true}) {
                unitOfWork.MessageRepository.DeleteMessage(message);
            }

            if(await unitOfWork.Complete()) return Ok();

            return BadRequest("Problem deleting the message");
    }
}
}