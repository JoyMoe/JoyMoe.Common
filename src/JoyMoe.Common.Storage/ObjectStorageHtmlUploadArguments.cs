using System.Collections.Generic;

namespace JoyMoe.Common.Storage;

public class ObjectStorageFrontendUploadArguments
{
    public string                     Action           { get; set; } = null!;
    public string                     Method           { get; set; } = "post";
    public string                     FieldContentType { get; set; } = "content-type";
    public string                     FieldFile        { get; set; } = "file";
    public Dictionary<string, string> Data             { get; }      = new();
}
