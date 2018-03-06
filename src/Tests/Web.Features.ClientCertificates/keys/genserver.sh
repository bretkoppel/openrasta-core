#!/bin/sh
rm certificates/server.*

echo "RSA Private Key (.pem)"
openssl genrsa -out ./certificates/server.pem 4096 -passout pass:openrasta

echo "Certificate Signing Request (.csr)"
openssl req -config videndel.conf \
      -subj "/C=MC/ST=Monaco/L=Monte-Carlo/O=Videndel/OU=Online Services/CN=openrasta.example" \
      -key ./certificates/server.pem \
      -new -sha256 \
      -out ./certificates/server.csr

echo "X509 Certificate (.cer)"
openssl ca \
      -config videndel.conf \
      -batch \
      -passin pass:openrasta \
      -extensions server_cert \
      -days 375 \
      -notext \
      -md sha256 \
      -in ./certificates/server.csr \
      -out ./certificates/server.cer

echo "PKCS12 Certificate (.pfx)"
 openssl pkcs12 -export -nodes -passin pass:openrasta -passout pass:openrasta -in ./certificates/server.cer -inkey ./certificates/server.pem -certfile ./certificates/ca.cer -out ./certificates/server.pfx 