FROM dockerhub.humanbacker.com/dotnet/core/aspnet-with-image:8.0.3 AS runtime
RUN sed -i 's/providers = provider_sect/providers = provider_sect\n\
ssl_conf = ssl_sect\n\
\n\
[ssl_sect]\n\
system_default = system_default_sect\n\
\n\
[system_default_sect]\n\
Options = UnsafeLegacyRenegotiation\n\
MinProtocol = TLSv1.2\n\
CipherString = DEFAULT@SECLEVEL=0/' /etc/ssl/openssl.cnf

WORKDIR /app
EXPOSE 80
COPY ./ ./
ENV LANG en_US.UTF-8
RUN echo "Asia/shanghai" > /etc/timezone
RUN cp /usr/share/zoneinfo/Asia/Shanghai /etc/localtime
ENTRYPOINT ["dotnet", "UserInfoManager.dll"]