<img src="./Wave/Assets/Wave%20Logo%20Transparent.png" alt="" width="300" /># Wave## The Open-Source Blogging Engine![](https://img.shields.io/github/license/miawinter98/Wave?color=green)![](https://img.shields.io/github/forks/miawinter98/Wave?label=github%20forks&logo=github)![](https://img.shields.io/github/stars/miawinter98/Wave?label=github%20stars&color=yellow&logo=github)![](https://img.shields.io/docker/pulls/miawinter/wave?color=informational&logo=docker)![](https://img.shields.io/docker/stars/miawinter/wave?color=yellow&logo=docker)⚠ Under Construction ⚠ alpha-17 image available for the brave.## QuickstartThis docker compose file will give you everything you need to run Wave. See the followingsections for explanations about the configuration and makeup of Wave. Replace <*_password>with generated passwords, just in case, replace your-time-zone with a sensible time zone for your users.For extensive configuration you want to mount `/configuration` to a location on your system.Afterwards you can access Wave on `http://localhost`.To see how to create an admin account, read the following section. Afterwards for securityyou should [Configure an Email Server](#configuring-email).```version: '3.4'name: waveservices:  web:    image: miawinter/wave:alpha-17    restart: unless-stopped    ports:      - "80:8080"    links:        - database:db    environment:      - "TZ=<your-time-zone>"      - "WAVE_ConnectionStrings__DefaultConnection=Host=db; Username=wave; Password=<db_password>"      - "WAVE_ConnectionStrings__Redis=redis,password=<redis_password>"    volumes:      - wave-files:/app/files      - wave-config:/configuration    networks:      - wave    depends_on:      - database  database:    image: postgres:16.1-alpine    restart: unless-stopped    environment:      - "POSTGRES_DB=wave"      - "POSTGRES_USER=wave"      - "POSTGRES_PASSWORD=<db_password>"    volumes:      -  wave-db:/var/lib/postgresql/data    networks:      - wave   redis:    image: redis:7-alpine    restart: unless-stopped    command: redis-server --requirepass <redis_password> --save 60 1 --loglevel warning    volumes:      - wave-redis:/data    networks:      - wavevolumes:  wave-files:  wave-config:  wave-db:  wave-redis:networks:  wave:```Note: when binding the files volume to a local directory, keep in mind Wave runs by default on an internal "app" user (`1654:1654`), not root. You need to adjust your directory permissions or docker compose file accordingly (or just slap `chmod -R 777` on it).### Admin AccessWhen Wave does not detect any admin account in its database on startup, which usually happens duringsetup, a message will be printed to it's server console, in docker accessible with `docker logs wave-web-1`:`There is currently no user in your installation with the admin role, go to /Admin and use the following password to self promote your account: [password]`The password is 16 digits long, navigate to `http://localhost/Admin`, if you are not logged in you will be redirected tothe login page. Once you are authenticated and have entered the password on the admin page, the tool will be disabled andyou will be a member of the admin role, giving you full access to all of Waves' features. ~~Keep in mind that the passwordis generated every time on startup as long as there is no admin, so if you restart the container, there will be a differentpassword in the console.~~ Since pull request #3 probably stays the same.## Configuring WaveWave allows you to configure it in many different formats and in multiple places, and you can even use multiple of the following methods to supply configuration information. Please keep in mind that first, asp.net configuration keys are case-insensitive, and second,that there is a precedence in the different formats, so a value for the same key in two formats will be overwritten by one.### Configuration Locations There are two main locations where Wave (and asp.net) takes its configuration from: The Environment, and the `/configuration` volume. Environment variables allow you to quickly set up a docker container, but the more you need to configure the more unmaintainable an `.env` file (or an `environment:` section in docker compose) becomes, so if you find yourself customizing a lot of Waves behaviour, consider using one of the many supported configuration file formats.### Configuration KeysI will provide you the different configuration keys with a dot notation, like `Email.Smtp.Host`.In environment variables, these dots need to be replaced with two underscore characters: `__`and prefixed with `WAVE_`.  In config files, those dots are hierarchy level, and you need to implement that dialects' syntax for it. Here some examples for `Email.Smtp.Host`:**Environment**```WAVE_Email__Smtp__Host=smtp.example.com```**JSON**```json{    "Email": {        "Smtp:": {            "Host": "smtp.example.com"        }    }}```**YAML**```ymlEmail:  Smtp:    Host: smtp.example.com```### Supported Configuration FormatsWave will take configuration from the following files in the `/configuration` volume, fileslater in this chain will have precedence over files earlier in that chain:- config.json- config.yml- config.toml- config.ini- config.xmlAfter this, values from the Environment will take the highest precedence. ## Configuring EmailWave may send user related mails every now and then, to confirm an account, reset a password, etc.In order to support that, Wave needs to have a way to send Emails, currently SMTP is supported.### SMTPThe following configuration is required for Wave to connect to a smtp server (formatted in YAML for brevity).```ymlEmail:  SenderEmail: noreply@example.com  SenderName: Wave  ServiceEmail: contact@example.com # used in various places, including email newsletter ListId Header  Smtp:    Live:      Host: smtp.example.com      Port: 25      Username: user      Password: password      Ssl: true````Username` and `Password` are optional if your server does not require it, and `Ssl` is `true` by default, only set it to false if you really need to, keeping security in mind.## RedisWave will generate a variety of keys for anti-forgery and logged in users during it's runtime. By default, these will be persisted into an in-memory key store, which will be lost when restarting the Wave container, causing all users to be logged out. To persistthese keys outside of the containers' lifetime, you can configure a Redis connection stringusing `ConnectionStrings.Redis`.## Reverse ProxyIn order to make your Wave installation available to the web, you want to use a reverse proxy to handle things like SSL Certificates. Here are some examples. ### CaddyIn the Caddyfile add:```<your domain> {  encode gzip zstd  @static path_regexp \.(js|css|svg|png|webp|jxl|jpg|jpeg|woff2)$  header ?Cache-Control "max-age=600"  header @static Cache-Control "max-age=3600"  header -server  reverse_proxy localhost:8080}```If Caddy runs as a docker container, you need to use the Wave container name instead of `localhost`.Adapt the Cache-Control header to your needs, this config sets a fallback of 600 (10 minutes) and 3600 (60 minutes) for static assets like fonts, images, js and css. Wave will set a Cache-Controlheader for certain files, only overwrite them if you know what you are doing.### Nginx TODO## RSSWave by default supports retrieving your articles with rss, both the vanilla and the atom flavour. The RSS endpoints are located at `/rss/rss.xml` and `/rss/atom.xml`. They also allow you to filter for a category with a `category` query parameter like so: `/rss/atom.xml?category=cats`.If for some reason you wish to disable these endpoints and RSS with it, set the configuration key `Features.RSS` to false. Please don't disable it because you think it will make your site more secure or less scrapable, it does not.## Email SubscriptionsWave can allow users to subscribe to E-Mail updates about new articles. In order to enable this feature you first need to set the configuration key `Features.EmailSubscriptions` to true. You also need to configureEmails in general, please follow the [Configure an Email Server](#configuring-email) Section for this.Besides this you will need to provide a mail distributor for these bulk E-Mails. This may be the same as you have already configured for live E-Mails, in this case just copy the configuration from there, butespecially at larger volumes it is advisable to separate these concerns, and many hosting providers for large mail distribution will provide you with two sets of credentials for this. If you have followed all the previous instructions, your E-Mail configuration may look like this:```ymlEmail:  SenderEmail: noreply@example.com  SenderName: Wave  ServiceEmail: contact@example.com # used in various places, including email newsletter ListId Header  Smtp:    Live:      Host: smtp.example.com      Port: 25      Username: user      Password: password      Ssl: true    Bulk:      Host: bulk.smtp.example.com      Port: 465      Username: bulk-user      Password: bulk-password```## CustomizationsTODO implement more customizations, add description.Currently supported:```ymlCustomization:  AppName: My cool blog  AppDescription: This is where I write about my cats  AppUrl: https://example.com  DefaultTheme: wave-dark  DefaultLanguage: "en-GB"  LogoLink: https://miawinter.de/img/logo.png  Footer: (c) 2024 [Mia Rose Winter](https://miawinter.de/)```## Additional NotesTODO ?## License and AttributionWave by [Mia Winter](https://miawinter.de/) is licensed under the [MIT License](https://en.wikipedia.org/wiki/MIT_License).  Copyright (c) 2024 Mia Rose Winter