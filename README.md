<img src="./Wave/Assets/Wave%20Logo%20Transparent.png" alt="" width="300" />

# Wave
## The Open Source Blogging Engine

![](https://img.shields.io/github/license/miawinter98/Wave?color=green)
![](https://img.shields.io/github/forks/miawinter98/Wave?label=github%20forks&logo=github)
![](https://img.shields.io/github/stars/miawinter98/Wave?label=github%20stars&color=yellow&logo=github)

Under Construction

## Quickstart

TODO

## Configuring Wave

Wave allows you to configure it in many different formats and in multiple places, and 
you can even use multiple of the following methods to supply configuration information. 
Please keep in mind that first, asp.net configuration keys are case-insensitive, and second,
that there is a precedence in the different formats, so a value for the same key in two 
formats will be overwritten by one.

### Configuration Locations 

There are two main locations where Wave (and asp.net) takes it's configuration from: 
The Environment, and the `/configuration` volume. Environment variables allow you to quickly 
set up a docker container, but the more you need to configure the more unmaintainable an 
`.env` file (or an `environment:` section in docker compose) becomes, so if you find yourself 
customizing a lot of Waves behavior, consider using one of the many supported configuration 
file formats.

### Configuration Keys

I will provide you the different configuration keys with a dot notation, like `Email.Smtp.Host`.
In environment variables, these dots need to be replaced with two underscore characters: `__`
and prefixed with `WAVE_`.  
In config files, those dots are hierarchy level, and you need to implement that dialects' 
syntax for it. Here some examples for `Email.Smtp.Host`:

**Environment**

```
WAVE_Email__Smtp__Host=smtp.example.com
```

**JSON**
```json
{
    "Email": {
        "Smtp:": {
            "Host": "smtp.example.com"
        }
    }
}
```

**YAML**

```yml
Email:
  Smtp:
    Host: smtp.example.com
```

### Supported Configuration Formats

Wave will take configuration from the following files in the `/configuration` volume, files
later in this chain will have precedence over files earlier in that chain:

- config.json
- config.yml
- config.toml
- config.ini
- config.xml

After this, values from the Environment will take the highest precedence. 

## Installation

TODO

## Configuring Email

Wave may send user related mails every now and then, to confirm an account, reset a password, etc.
In order to support that, Wave needs to have a way to send Emails, currently SMTP is supported

### SMTP

The following configuration is required for Wave to connect to an smtp server 
(formatted in YAML for brevity).

```yml
Email:
  Smtp:
    Host: smtp.example.com
    Port: 25
    SenderEmail: noreply@example.com
    SenderName: Wave
    Username: user
    Password: password
    Ssl: true
```

`Username` and `Password` are optional if your server does not require it, and `Ssl` is 
`true` by default, only set it to false if you really need to, keeping security in mind.

## License and Attribution

Wave by [Mia Winter](https://miawinter.de/) is licensed under the [MIT License](https://en.wikipedia.org/wiki/MIT_License).  

Copyright (c) 2024 Mia Rose Winter