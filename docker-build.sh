tag=$(date +"%Y%m%d%H%M")
docker build -t rinha-2024q1-crebito-dotnet:$tag -t zanfranceschi/rinha-2024q1-crebito-dotnet:$tag .
docker push zanfranceschi/rinha-2024q1-crebito-dotnet:$tag

echo $tag