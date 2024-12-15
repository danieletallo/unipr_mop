using AutoMapper;
using Payments.Repository.Model;
using Payments.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Payments.Business.Profiles
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
            CreateMap<PaymentInsertDto, Payment>().ReverseMap();
            CreateMap<PaymentUpdateDto, Payment>().ReverseMap();
            CreateMap<Payment, PaymentReadDto>().ReverseMap();
        }
    }
}
