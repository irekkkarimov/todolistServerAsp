using AutoMapper;
using todolistServer.Models;
using Task = todolistServer.Models.Task;

namespace todolistServer.DTOs;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<TaskCreateDto, Task>();
        CreateMap<TaskDto, Task>();
        CreateMap<Task, TaskDto>();
        CreateMap<UserDto, User>();
        CreateMap<User, UserDto>();
        CreateMap<UserLoginDto, User>();
        CreateMap<UserEditDto, User>();
    }
}