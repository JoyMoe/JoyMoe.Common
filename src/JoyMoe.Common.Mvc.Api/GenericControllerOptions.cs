using System;
using System.Collections.Generic;
using AutoMapper;

namespace JoyMoe.Common.Mvc.Api
{
    public class GenericControllerOptions
    {
        public List<GenericControllerType> Types { get; } = new List<GenericControllerType>();
        public List<Profile> Profiles { get; } = new List<Profile>();
    }

    public class GenericControllerType
    {
        public Type EntityType { get; set; } = null!;
        public Type? RequestType { get; set; }
        public Type? ResponseType { get; set; }
    }
}
