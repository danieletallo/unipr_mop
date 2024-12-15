using AutoMapper;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Repository.Model;
using Warehouse.Shared;

namespace Warehouse.Business.Profiles
{
    public sealed class AssemblyMarker
    {
        AssemblyMarker() { }
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class InputFileProfile : Profile
    {
        public InputFileProfile()
        {
            CreateMap<ItemInsertDto, Item>().ReverseMap();
            CreateMap<ItemUpdateDto, Item>().ReverseMap();
            CreateMap<Item, ItemReadDto>().ReverseMap();
            CreateMap<ItemHistory, ItemHistoryReadDto>().ReverseMap();
        }
    }
}
