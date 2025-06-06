This project is a small e-commerce application built using a microservices architecture (4 microservices). It manages functionalities such as order insertion, online payments, inventory management, and supplier integration.

Project Structure:
- **API**: Exposes public endpoints for communication.
- **Business**: Contains the core business logic for processing operations.
- **ClientHTTP**: Handles HTTP communication between microservices.
- **Repository**: Manages data access and interactions with the database.
- **Shared**: Includes shared components, DTOs, and utilities.

Added **Kafka** for asynchronous communication.

Created 4 Kafka topics:

- "order-created": Produced by OrdersService and consumed by PaymentsService and by WarehouseService.
- "payment-status-changed": Produced by PaymentsService and consumed by OrdersService and WarehouseService.
- "customer-created": Produced by RegistryService and consumed by OrdersService.
- "supplier-created": Produced by RegistryService and consumed by WarehouseService.
