# Docker-Compose

Start root repository
```
docker-compose up -d --build
```

Stop root repository
```
docker-compose down
``` 

## EndPoints
- [DynamoDB](http://localhost:8000)  
- [DynamoDB UI](http://localhost:8001)
- [DynamoDB Test API](https://localhost:5000/swagger/index.html)

# Debug
Enter VS and debug with docker compose

# Manual Test
```
 aws dynamodb list-tables --endpoint-url http://localhost:8000
```

