# Embeddings & Self-Attention for Anomaly Detection in Tabular Data

This repository demonstrates a practical approach to detecting anomalies in tabular data by repurposing **Transformer-inspired** concepts like embeddings and self-attention. Built in **C#** with a clear **Domain-Driven Design (DDD)** structure, it focuses on:

- **Automatic normalization** of numeric fields via order-of-magnitude scaling.  
- **Hash-based embeddings** for discrete fields, transforming categories into short, stable vectors.  
- **Dimension-wise z‑score analysis** to identify which fields drive anomalies.  
- **Parallel processing** for efficient handling of large data sets.

---

## Overview

1. **Embedding Discrete Fields**  
   Converts categorical values (like `Department`, `JobTitle`) into fixed-size continuous vectors using a hashing function (SHA‑256). No training or large embedding models needed.

2. **Normalization of Numeric Fields**  
   Automatically adjusts each numeric field (e.g., `Salary`, `Age`) by its order of magnitude, preserving outliers without cumbersome min–max configs.

3. **Dimension-Wise Outlier Detection**  
   After computing a “row embedding,” the code groups rows (e.g., by `Department`) and calculates mean/std. dev. embeddings. Then, **z‑scores** pinpoint anomalies dimension by dimension.

4. **Explainability**  
   Once a row is flagged, we re-check each field’s embedding to see which dimension/field contributed most to the anomaly. This mirrors a self-attention–style focus.

---

## Key Features

- **DDD Architecture**: Code is organized into **Domain**, **Application**, **Infrastructure**, and **Presentation** layers.  
- **Parallel Processing**: Uses `Task.Run(...)` and concurrency structures for faster execution on large datasets.  
- **Hash & Normalize**: A stable, zero-configuration approach for embedding discrete fields and scaling numeric fields.  
- **Explanatory Logs**: Logs detail which field triggered the anomaly, along with dimension-wise z‑scores for transparency.

---

1. **Domain**: Contains `Employee`, `EmbeddingVector`, `IEmbeddingService`, normalization, and anomaly detection logic.  
2. **Application**: Defines commands (e.g., `ProcessEmployeeDataCommand`) and services (`EmployeeProcessingService`) that drive the domain operations.  
3. **Infrastructure**: Responsible for data I/O (e.g., CSV loading via `CsvHelper`), plus any external resources.  
4. **Presentation**: Entry point (`Program.cs`) and controllers (e.g., `EmployeeController`) that wire everything together.

---

## Getting Started

1. **Clone the Repository**  
   ```bash
   git clone https://github.com/YourUserName/EmbeddingsSelfAttentionAnomaly.git
   cd EmbeddingsSelfAttentionAnomaly

2. **Update Program.cs or EmployeeController with the path to your CSV (default is "data.csv" - (bin directory))**

## Usage
Load Employees
The CsvEmployeeLoader reads your CSV asynchronously, creating a list of Employee objects.

Compute Embeddings
The EmployeeProcessingService uses SimpleEmbeddingService to hash discrete fields (department, job title, etc.) and Normalizer to scale numeric fields.

Group & Detect Anomalies
Employees are grouped by Department (and then JobTitle) to compute group-level mean and std. dev. Embeddings. Each row’s embedding is checked dimension by dimension using z‑scores.

Log Anomalies
The application logs anomalies to the console, detailing which field caused the most significant deviation.


## Contributing
We welcome contributions! Feel free to open an issue or submit a pull request for bug fixes, feature ideas, or expansions—especially if you want to add advanced embedding logic, additional grouping strategies, or performance optimizations.

## License
This project is licensed under the MIT License. You’re free to use, modify, and distribute this code in your own projects.

## References
Vaswani, A. et al. (2017). Attention Is All You Need.
Evans, E. (2003). Domain-Driven Design: Tackling Complexity in the Heart of Software.
Chandola, V., Banerjee, A., & Kumar, V. (2009). Anomaly Detection: A Survey.

## Questions?
Feel free to open an issue or let me know if you have any comments, questions, or suggestions. Enjoy exploring Transformer-like embeddings for your tabular data anomalies!
