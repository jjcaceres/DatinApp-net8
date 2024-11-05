using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace API.Controllers
{

    [Authorize]
    public class UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService) : BaseApiController
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {

            var users = await userRepository.GetMembersAsync();

            return Ok(users);
        }
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {

            var user = await userRepository.GetMemberAsync(username);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }
        [HttpPut]
        public async Task<ActionResult> Update(MemberUpdateDto memberUpdateDto)
        {

            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user is null) return BadRequest("Could not find user");

            mapper.Map(memberUpdateDto, user);

            if (await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update the user");

        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user is null) return BadRequest("Cannot update user");

            var result = await photoService.AddPhotoAync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId

            };
            user.Photos.Add(photo);
            if (await userRepository.SaveAllAsync()) return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, mapper.Map<PhotoDto>(photo));
            //return mapper.Map<PhotoDto>(photo);
            return BadRequest("Problem adding photo");

        }
        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> setMainPhoto(int photoId)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user is null) return BadRequest("Could not find user");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo is null || photo.IsMain) return BadRequest("Cannot use this as main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain is not null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting main photo");

        }
        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user is null) return BadRequest("Could not find user");
            var photo = user.Photos.FirstOrDefault(x=>x.Id== photoId);
            if(photo is null || photo.IsMain) return BadRequest("This photo cannot be deleted");

            if(photo.PublicId is not null)
            {
                var result = await photoService.DeletePhotoAync(photo.PublicId);
                if(result.Error is not null) return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo);

            if(await userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting photo");

        }

    }
}
