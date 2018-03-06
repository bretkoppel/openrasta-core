#!/bin/sh
rm certificates/client.*

echo "RSA Private Key (.pem)"
openssl genrsa -out ./certificates/client.pem 4096 -passout pass:openrasta

echo "Certificate Signing Request (.csr)"
openssl req -config videndel.conf \
      -subj "/C=MC/ST=Monaco/L=Monte-Carlo/O=Videndel/OU=Online Services/CN=0.0.0.0" \
      -key ./certificates/client.pem \
      -new -sha256 \
      -out ./certificates/client.csr

echo "X509 Certificate (.cer)"
openssl ca \
      -config videndel.conf \
      -batch \
      -passin pass:openrasta \
      -extensions usr_cert \
      -days 375 \
      -notext \
      -md sha256 \
      -in ./certificates/client.csr \
      -out ./certificates/client.cer

echo "PKCS12 Certificate (.pfx)"
openssl pkcs12 -export -nodes -passin pass:openrasta -passout pass:openrasta -in ./certificates/client.cer -inkey ./certificates/client.pem -certfile ./certificates/ca.cer -out ./certificates/client.pfx 