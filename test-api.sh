#!/bin/bash

echo "================================"
echo "CDB Calculator - Local Test"
echo "================================"
echo ""

echo "Testing CDB Calculation API..."
echo ""

# Test 1: Calculate CDB for 1000 BRL over 12 months
echo "Test 1: Calculate 1000 BRL over 12 months"
curl -X POST http://localhost:8080/api/cdbcalculator/calculate \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: test-correlation-123" \
  -d '{
    "initialValue": 1000,
    "months": 12
  }' | jq '.'

echo ""
echo ""

# Test 2: Calculate CDB for 5000 BRL over 6 months
echo "Test 2: Calculate 5000 BRL over 6 months (22.5% IR)"
curl -X POST http://localhost:8080/api/cdbcalculator/calculate \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: test-correlation-456" \
  -d '{
    "initialValue": 5000,
    "months": 6
  }' | jq '.'

echo ""
echo ""

# Test 3: Calculate CDB for 10000 BRL over 36 months
echo "Test 3: Calculate 10000 BRL over 36 months (15% IR)"
curl -X POST http://localhost:8080/api/cdbcalculator/calculate \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: test-correlation-789" \
  -d '{
    "initialValue": 10000,
    "months": 36
  }' | jq '.'

echo ""
echo ""
echo "✅ Tests completed!"
