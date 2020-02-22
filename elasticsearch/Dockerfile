FROM byrnedo/alpine-curl:latest
WORKDIR /scripts
COPY create-index.sh /scripts
RUN chmod +x /scripts/create-index.sh
ENTRYPOINT ["sh", "/scripts/create-index.sh"]