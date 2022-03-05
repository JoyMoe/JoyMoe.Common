# JoyMoe.Common

Common Extensions & Services for Asp.Net Core from JoyMoe.

![GitHub Workflow Status](https://img.shields.io/github/workflow/status/JoyMoe/JoyMoe.Common/build)
[![Codecov](https://img.shields.io/codecov/c/github/JoyMoe/JoyMoe.Common.svg)](https://codecov.io/gh/JoyMoe/JoyMoe.Common)
[![license](https://img.shields.io/github/license/JoyMoe/JoyMoe.Common.svg)](https://github.com/JoyMoe/JoyMoe.Common/blob/master/LICENSE)
![netstandard2.1](https://img.shields.io/badge/.Net-netstandard2.1-brightgreen.svg)
![net3.1](https://img.shields.io/badge/.Net-3.1-brightgreen.svg)
![net6.0](https://img.shields.io/badge/.Net-6.0-brightgreen.svg)

![joymoe](joymoe.png)

| Package                          | Release                                                                                                                                          | Latest                                                                                                                                                             | Downloads                                                                                                                                         |
| -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| JoyMoe.Common.Abstractions       | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Abstractions.svg)](https://www.nuget.org/packages/JoyMoe.Common.Abstractions)             | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Abstractions.svg)](https://www.nuget.org/packages/JoyMoe.Common.Abstractions/absoluteLatest)             | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Abstractions.svg)](https://www.nuget.org/packages/JoyMoe.Common.Abstractions)             |
| JoyMoe.Common.Attributes         | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Attributes.svg)](https://www.nuget.org/packages/JoyMoe.Common.Attributes)                 | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Attributes.svg)](https://www.nuget.org/packages/JoyMoe.Common.Attributes/absoluteLatest)                 | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Attributes.svg)](https://www.nuget.org/packages/JoyMoe.Common.Attributes)                 |
| JoyMoe.Common.Data               | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Data.svg)](https://www.nuget.org/packages/JoyMoe.Common.Data)                             | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Data.svg)](https://www.nuget.org/packages/JoyMoe.Common.Data/absoluteLatest)                             | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Data.svg)](https://www.nuget.org/packages/JoyMoe.Common.Data)                             |
| JoyMoe.Common.Data.Dapper        | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Data.Dapper.svg)](https://www.nuget.org/packages/JoyMoe.Common.Data.Dapper)               | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Data.Dapper.svg)](https://www.nuget.org/packages/JoyMoe.Common.Data.Dapper/absoluteLatest)               | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Data.Dapper.svg)](https://www.nuget.org/packages/JoyMoe.Common.Data.Dapper)               |
| JoyMoe.Common.Data.EFCore        | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Data.EFCore.svg)](https://www.nuget.org/packages/JoyMoe.Common.Data.EFCore)               | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Data.EFCore.svg)](https://www.nuget.org/packages/JoyMoe.Common.Data.EFCore/absoluteLatest)               | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Data.EFCore.svg)](https://www.nuget.org/packages/JoyMoe.Common.Data.EFCore)               |
| JoyMoe.Common.Data.LinqToDB        | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Data.LinqToDB.svg)](https://www.nuget.org/packages/JoyMoe.Common.Data.LinqToDB)               | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Data.LinqToDB.svg)](https://www.nuget.org/packages/JoyMoe.Common.Data.LinqToDB/absoluteLatest)               | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Data.LinqToDB.svg)](https://www.nuget.org/packages/JoyMoe.Common.Data.LinqToDB)               |
| JoyMoe.Common.Diagnostics        | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Diagnostics.svg)](https://www.nuget.org/packages/JoyMoe.Common.Diagnostics)               | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Diagnostics.svg)](https://www.nuget.org/packages/JoyMoe.Common.Diagnostics/absoluteLatest)               | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Diagnostics.svg)](https://www.nuget.org/packages/JoyMoe.Common.Diagnostics)               |
| JoyMoe.Common.Json               | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Json.svg)](https://www.nuget.org/packages/JoyMoe.Common.Json)                             | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Json.svg)](https://www.nuget.org/packages/JoyMoe.Common.Json/absoluteLatest)                             | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Json.svg)](https://www.nuget.org/packages/JoyMoe.Common.Json)                             |
| JoyMoe.Common.Mvc.Api            | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Mvc.Api.svg)](https://www.nuget.org/packages/JoyMoe.Common.Mvc.Api)                       | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Mvc.Api.svg)](https://www.nuget.org/packages/JoyMoe.Common.Mvc.Api/absoluteLatest)                       | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Mvc.Api.svg)](https://www.nuget.org/packages/JoyMoe.Common.Mvc.Api)                       |
| JoyMoe.Common.Session            | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Session.svg)](https://www.nuget.org/packages/JoyMoe.Common.Session)                       | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Session.svg)](https://www.nuget.org/packages/JoyMoe.Common.Session/absoluteLatest)                       | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Session.svg)](https://www.nuget.org/packages/JoyMoe.Common.Session)                       |
| JoyMoe.Common.Session.Repository | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Session.Repository.svg)](https://www.nuget.org/packages/JoyMoe.Common.Session.Repository) | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Session.Repository.svg)](https://www.nuget.org/packages/JoyMoe.Common.Session.Repository/absoluteLatest) | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Session.Repository.svg)](https://www.nuget.org/packages/JoyMoe.Common.Session.Repository) |
| JoyMoe.Common.Storage            | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Storage.svg)](https://www.nuget.org/packages/JoyMoe.Common.Storage)                       | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Storage.svg)](https://www.nuget.org/packages/JoyMoe.Common.Storage/absoluteLatest)                       | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Storage.svg)](https://www.nuget.org/packages/JoyMoe.Common.Storage)                       |
| JoyMoe.Common.Storage.QCloud     | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Storage.QCloud.svg)](https://www.nuget.org/packages/JoyMoe.Common.Storage.QCloud)         | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Storage.QCloud.svg)](https://www.nuget.org/packages/JoyMoe.Common.Storage.QCloud/absoluteLatest)         | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Storage.QCloud.svg)](https://www.nuget.org/packages/JoyMoe.Common.Storage.QCloud)         |
| JoyMoe.Common.Storage.S3         | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Storage.S3.svg)](https://www.nuget.org/packages/JoyMoe.Common.Storage.S3)                 | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Storage.S3.svg)](https://www.nuget.org/packages/JoyMoe.Common.Storage.S3/absoluteLatest)                 | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Storage.S3.svg)](https://www.nuget.org/packages/JoyMoe.Common.Storage.S3)                 |
| JoyMoe.Common.Workflow           | [![NuGet](https://img.shields.io/nuget/v/JoyMoe.Common.Workflow.svg)](https://www.nuget.org/packages/JoyMoe.Common.Workflow)                     | [![NuGet](https://img.shields.io/nuget/vpre/JoyMoe.Common.Workflow.svg)](https://www.nuget.org/packages/JoyMoe.Common.Workflow/absoluteLatest)                     | [![NuGet](https://img.shields.io/nuget/dt/JoyMoe.Common.Workflow.svg)](https://www.nuget.org/packages/JoyMoe.Common.Workflow)                     |

## License

The MIT License

More info see [LICENSE](LICENSE)
