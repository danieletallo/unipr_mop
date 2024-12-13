using System;
using System.Collections.Generic;
using AutoMapper;
using System.Diagnostics.CodeAnalysis;
using Orders.Repository.Model;
using Orders.Shared;

namespace Orders.Business.Profiles
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
            CreateMap<OrderInsertDto, Order>().ReverseMap();
            CreateMap<OrderDetailInsertDto, OrderDetail>().ReverseMap();
            CreateMap<Order, OrderReadDto>().ReverseMap();
            CreateMap<OrderDetail, OrderDetailReadDto>().ReverseMap();
        }
    }
}
