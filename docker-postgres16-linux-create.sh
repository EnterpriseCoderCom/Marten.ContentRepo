sudo docker pull postgres:16
sudo docker create -p 29123:5432 -e POSTGRES_USERNAME=postgres -e POSTGRES_PASSWORD=3nterp4is3C0de4 --name enterprise-coder-pgsql-ut postgres:16
