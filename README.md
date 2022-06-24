# .NET on Kubernetes

This is a demo repo to demonstrate techniques for building .NET services for Kubernetes

Using with Docker Desktop with Kubernetes installed:

1. Build the container image:

`docker build -t greetings-service:0.0.1 .`

2. Deploy to Kubernetes:

`kubectl apply -f greetings-service-deployment.yaml`