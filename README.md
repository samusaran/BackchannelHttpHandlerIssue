# BackchannelHttpHandler issue repro project

This project serves as a repro project for an issue with OIDC running behind a corporate proxy.

## How to use

- Set the correct AzureAD settings under the AzureAD section of the config
- Run a proxy (fiddler is perfect, be sure to disable `Act as a system proxy on startup`)
- At `Program.cs:37` you will find a commented line. Comment\Uncomment this line to see the different behaviour.

## Result

When authenticating a user...
- ... without using `HttpClient.DefaultProxy`, you will see 2 calls:
  - `/<tenant-id>/v2.0/.well-known/openid-configuration` 
  - `/<tenant-id>/discovery/v2.0/keys`
- ... while using `HttpClient.DefaultProxy` you will see 4 calls:
  - `/<tenant-id>/v2.0/.well-known/openid-configuration`
  - `/<tenant-id>/discovery/v2.0/keys`
  - `/common/discovery/instance?....`
  - `/<tenant-id>/oauth2/v2.0/token`

This means that setting the proxy on `BackchannelHttpHandler` is not sufficient when using OIDC behind corporate proxy.