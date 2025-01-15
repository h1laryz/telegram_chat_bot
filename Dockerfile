FROM ubuntu:24.10

WORKDIR /install

RUN apt-get -y update && apt-get -y upgrade
RUN apt-get install -y sudo

ENV DEBIAN_FRONTEND=noninteractive
RUN ln -fs /usr/share/zoneinfo/UTC /etc/localtime && \
    apt install --quiet --yes --no-install-recommends tzdata && \
    dpkg-reconfigure --frontend noninteractive tzdata

COPY scripts scripts

RUN ./scripts/install_env.sh

#EXPOSE "8080"