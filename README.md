Granulometric Analysis for Mineral Processing

Overview
This project is a C# console application designed to perform granulometric analysis, a fundamental process in mineral engineering.
It calculates key metrics such as particle size distribution, characteristic diameters (D10, D50, D60), and classification coefficients (Uniformity and Curvature). 
The results provide critical insights into a mineral sample's particle size, which is essential for optimizing comminution (crushing and grinding) and mineral concentration processes.

Features

  1. Data Input
  2. The application supports two convenient methods for data entry:
  3. Manual Entry: Users can input sieve size and retained mass directly via the console.
  4. CSV File: The application can read data from a comma-separated values (CSV) file.

Calculations

The program performs a series of calculations to analyze the particle size data:

1. Total mass of the sample.
2. Percentage of retained, accumulated, and passing material.
3. Characteristic diameters (D10, D30, D50, D60, D90) via linear interpolation.
4. Classification coefficients: Uniformity (C_u) and Curvature (C_c).

Results

  1. The output provides a comprehensive analysis of the sample:
  2. A detailed table showing all calculated percentages.
  3. A clear display of key characteristic diameters and coefficients.
  4. A detailed interpretation of the results from a mineral processing perspective.
  5. A simple text-based granulometric curve (D50 graph) to visualize the data.
