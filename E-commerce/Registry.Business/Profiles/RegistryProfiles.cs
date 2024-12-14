using AutoMapper;
using Registry.Repository.Model;
using Registry.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Registry.Business.Profiles
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
            CreateMap<CustomerInsertDto, Customer>().ReverseMap();
            CreateMap<Customer, CustomerReadDto>().ReverseMap();
            CreateMap<SupplierInsertDto, Supplier>().ReverseMap();
            CreateMap<Supplier, SupplierReadDto>().ReverseMap();
        }
    }
}
