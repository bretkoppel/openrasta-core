#!/bin/sh
rm -rf certificates
mkdir certificates
rm -rf ca
mkdir ca ca/crl ca/newcerts ca/private ca/newcerts
touch ca/index.txt ca/serial
echo "00" >> ca/serial

echo "RSA Private Key (.pem)"
openssl genrsa -aes256 -passout pass:openrasta -out ./certificates/ca.pem 4096
chmod 400 ./certificates/ca.pem

echo "X509 Certificate (.cer)"
openssl req -config videndel.conf \
      -subj "/C=MC/ST=Monaco/L=Monte-Carlo/O=Videndel/OU=Online Services/CN=OpenRasta Tests Certificate Authority" \
      -passin pass:openrasta \
      -key ./certificates/ca.pem \
      -new -x509 -days 7300 -sha256 -extensions v3_ca \
      -out ./certificates/ca.cer
