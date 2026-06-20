// The library's own HttpMethod enum collides by name with System.Net.Http.HttpMethod
// (brought in by implicit usings). Every reference to the System type is fully
// qualified, so aliasing the bare name to our enum keeps service declarations terse.
global using HttpMethod = Nexsure.Api.Enums.HttpMethod;
