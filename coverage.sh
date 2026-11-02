#!/bin/bash

set -e

echo "Limpando resultados anteriores..."

rm -rf tests/EloVet.UnitTests/TestResults
rm -rf tests/EloVet.IntegrationTests/TestResults
rm -rf coveragereport

echo "Executando testes e coletando cobertura..."

dotnet test --collect:"XPlat Code Coverage"

echo "Gerando relatório de cobertura..."

reportgenerator \
  "-reports:tests/EloVet.IntegrationTests/TestResults/*/coverage.cobertura.xml;tests/EloVet.UnitTests/TestResults/*/coverage.cobertura.xml" \
  "-targetdir:coveragereport" \
  "-reporttypes:Html"

echo "Abrindo relatório atualizado..."

open coveragereport/index.html