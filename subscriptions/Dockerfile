FROM mesosphere/aws-cli
WORKDIR /scripts
COPY config.sh /scripts
RUN chmod +x /scripts/config.sh
ENTRYPOINT ["sh", "/scripts/config.sh"]
