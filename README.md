# .NET Microservices: Platform & Commands Service (.NET 8)

![.NET](https://img.shields.io/badge/.NET-8.0-512bd4?style=flat&logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ed?style=flat&logo=docker)
![Kubernetes](https://img.shields.io/badge/Kubernetes-Ready-326ce5?style=flat&logo=kubernetes)

This repository contains the source code for the microservices application built in Les Jackson's **[.NET Microservices – Full Course](https://www.youtube.com/watch?v=DgVjEo3OGBI)**.

**Update:** This project has been upgraded from the original .NET 5 implementation to **.NET 8**, utilizing the latest NuGet packages and modern C# features.

## 📋 Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Technologies Used](#technologies-used)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Credits](#credits)

## 🔭 Overview

The application demonstrates a microservices architecture comprising two distinct services that communicate both synchronously and asynchronously:

1.  **Platform Service**: The "source of truth" service. It manages IT platforms and stores data in a SQL Server database. It publishes events via RabbitMQ when a platform is published.
2.  **Commands Service**: A consumer service. It manages command-line snippets associated with platforms. It consumes events from RabbitMQ to keep its internal data in sync and uses gRPC for bulk data fetches.

## 🏗 Architecture

The solution implements several key microservices patterns:
* **Database per Service**: PlatformService uses SQL Server; CommandsService uses an In-Memory DB.
* **Sync Communication**: HTTP (REST) for external clients; gRPC for internal service-to-service communication.
* **Async Communication**: RabbitMQ for event-driven data consistency (Message Bus).
* **Gateway**: NGINX Ingress Controller for routing traffic to the cluster.

## 🛠 Technologies Used

* **Framework**: .NET 8 (C# 12)
* **Containerization**: Docker
* **Orchestration**: Kubernetes (K8s)
* **Messaging**: RabbitMQ
* **RPC**: gRPC
* **Data Access**: Entity Framework Core
* **Database**: SQL Server (Linux container) & In-Memory
* **Reverse Proxy**: NGINX Ingress

## ⚙️ Prerequisites

To run this project locally, ensure you have the following installed:

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop) (with Kubernetes enabled)
* A tool for API testing (Postman, Insomnia, etc.)

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/abdulrahmanbfh/PlatformCommands-Microservices
cd PlatformCommands-Microservices

## 2. Build Docker Images

You must build the images locally so Kubernetes can pull them (or push them to your Docker Hub and update the YAML files).

```bash
docker build -t <your-dockerhub-user>/platformservice -f PlatformService/Dockerfile .
docker build -t <your-dockerhub-user>/commandservice -f CommandsService/Dockerfile .
```

> Note:  
> Replace <your-dockerhub-user> with your actual Docker Hub username.  
> Ensure the image names match what is defined in your K8s deployment files (platforms-depl.yaml, etc.).


## 3. Deploy to Kubernetes

Navigate to the Kubernetes configuration folder and apply the manifests:

```bash
cd K8S
kubectl apply -f platforms-depl.yaml
kubectl apply -f commands-depl.yaml
kubectl apply -f rabbitmq-depl.yaml
kubectl apply -f mssql-plat-depl.yaml
kubectl apply -f ingress-srv.yaml
```

---

## 4. Configure Networking (Ingress)

If you are running locally (e.g., Docker Desktop), you likely need to map the host defined in ingress-srv.yaml to localhost.

Add the following to your hosts file (C:\Windows\System32\drivers\etc\hosts or /etc/hosts):

```plaintext
127.0.0.1 acme.com
```

---

## 5. Verify Deployment

Check that all pods are running:

```bash
kubectl get pods
```

---

## 📡 API Endpoints

Once running, you can access the services via the Ingress host (e.g., http://acme.com).

### Platform Service

| Method | Endpoint                | Description                                       |
|-------|-------------------------|---------------------------------------------------|
| GET   | /api/platforms          | Get all platforms                                 |
| GET   | /api/platforms/\{id}     | Get a specific platform                           |
| POST  | /api/platforms          | Create a platform (Triggers RabbitMQ event)       |

---

### Commands Service

| Method | Endpoint                                  | Description                                               |
|-------|--------------------------------------------|-----------------------------------------------------------|
| GET   | /api/c/platforms                           | Get all platforms (synced via RabbitMQ/gRPC)              |
| POST  | /api/c/platforms/\{id}/commands             | Add a command to a platform                               |
| GET   | /api/c/platforms/\{id}/commands             | Get commands for a platform                               |

---

## 👏 Credits

This project is based on the tutorial by Les Jackson.

- Original Tutorial: https://www.youtube.com/watch?v=DgVjEo3OGBI&pp=ygULbGVzIGphY2tzb27YBu0E
- Original Repo: https://github.com/binarythistle/S04E03---.NET-Microservices-Course-

Changes in this repository include updating the target framework to .NET 8 and updating all dependencies to their latest stable versions.