using AutoMapper;
using Maham.Domain.Entities;
using Maham.Application.DTOs.Auth;
using Maham.Application.DTOs.Project;
using Maham.Application.DTOs.Board;
using Maham.Application.DTOs.Column;
using Maham.Application.DTOs.Card;
using Maham.Application.DTOs.Comment;
using Maham.Application.DTOs.Attachment;
using Maham.Application.DTOs.Activity;

namespace Maham.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserResponseDto>();

        CreateMap<Project, ProjectResponseDto>();

        CreateMap<ProjectMember, ProjectMemberResponseDto>();

        CreateMap<Board, BoardResponseDto>();
        CreateMap<Board, BoardDetailsDto>();

        CreateMap<Column, ColumnResponseDto>();

        CreateMap<Card, CardResponseDto>();

        CreateMap<Comment, CommentResponseDto>();

        CreateMap<Attachment, AttachmentResponseDto>();

        CreateMap<ActivityLog, ActivityLogResponseDto>();
    }
}
