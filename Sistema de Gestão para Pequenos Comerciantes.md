
# Projeto: Sistema de Gestão para Pequenos Comerciantes

## Visão Geral

Este projeto tem como objetivo criar um sistema de gestão comercial inicialmente focado em pequenos comerciantes de alimentos, começando pelo segmento de salgados.

A ideia nasceu de uma necessidade real: ajudar uma comerciante que produz e vende salgados a organizar sua produção, pedidos, custos e entregas.

O sistema deve nascer simples, porém com arquitetura preparada para crescer futuramente para outros segmentos como:
- pizzarias
- lanchonetes
- pequenos mercados
- outros comércios locais

O objetivo não é criar um marketplace ou um sistema complexo inicialmente.
O foco é criar uma ferramenta de controle operacional.

---

# Contexto do Negócio Inicial

A primeira validação será com uma comerciante de salgados.

Ela trabalha com produtos como:

- salgados fritos
- salgados assados

Ela recebe pedidos em quantidades variadas:

Exemplos:
- 100 unidades
- 200 unidades
- 300 unidades
- pedidos personalizados

O sistema deve ajudar a controlar:

- quais produtos foram pedidos
- quantidade
- custo
- preço
- produção
- status do pedido
- planejamento da entrega

---

# Ideia Inicial Separada (Referência)

Existe uma primeira ideia relacionada a eventos locais.

Ela envolve:

- eventos recorrentes
- quantidade previsível de pedidos
- planejamento antecipado

Essa ideia NÃO será o foco principal do sistema.

Ela serve apenas como referência para entender:

- demanda
- previsão
- quantidade necessária
- custo de produção

O sistema principal deve ser genérico para comércio.

---

# Objetivo do MVP

O MVP deve resolver:

## Cadastro de Produtos

CRUD completo:

Produto:

- Id
- Nome
- Categoria
- Tipo (Frito / Assado)
- Descrição
- Foto
- Código de barras (futuro)
- Preço de venda
- Custo estimado
- Ativo/Inativo


---

## Cadastro de Clientes

Cliente:

- Nome
- Telefone
- Endereço
- Observações

Preparar para futura expansão.

---

# Fluxo Principal

## 1 - Cadastro

A comerciante cadastra:

- produtos
- preços
- categorias

---

## 2 - Pedido

Usuário seleciona produtos.

Pode escolher:

Produto individual:

Exemplo:

20 Coxinhas
30 Risoles

ou pacote:

100 salgados
200 salgados
300 salgados


---

## 3 - Carrinho

O sistema deve montar:

Pedido:

Cliente

Itens:

Produto
Quantidade
Valor unitário
Total


---

## 4 - Envio do Pedido

Ao confirmar:

Pedido fica disponível para a comerciante.

Status inicial:

AGUARDANDO


Fluxo:

AGUARDANDO
↓
EM PRODUÇÃO
↓
PRONTO
↓
ENTREGUE
↓
FINALIZADO


---

# Controle da Produção

A comerciante deve conseguir visualizar:

Quantidade total solicitada.

Exemplo:

Pedido:

200 salgados

Itens:

50 Coxinhas
50 Risoles
100 Bolinhas


Dashboard:

Total produzido:
0

Em produção:
200

Finalizado:
0


---

# Arquitetura

## Backend

Obrigatoriamente:

.NET / C#

O backend deve permanecer em .NET.

Tecnologia de interface pode mudar no futuro.

---

# Banco de Dados

Inicialmente:

SQL Server


Motivos:

- experiência do desenvolvedor
- robustez
- fácil evolução
- suporte empresarial


---

# Arquitetura de Software

Seguir princípios:

- Clean Architecture
- SOLID
- DDD quando fizer sentido
- Separação de responsabilidades
- Baixo acoplamento


Estrutura sugerida:

src/

Domain

Responsável por:

- entidades
- regras de negócio
- objetos de valor


Application

Responsável por:

- casos de uso
- serviços
- DTOs
- interfaces


Infrastructure

Responsável por:

- banco de dados
- EF Core
- implementações


API/UI

Responsável por:

- entrada do usuário
- comunicação


---

# Interface

Inicialmente:

Blazor

Porém não criar acoplamento.

A interface deve poder futuramente ser:

- Blazor
- Angular
- React
- Windows Forms


O backend deve continuar funcionando independente da interface.

---

# Princípios de Desenvolvimento

Antes de criar código:

Avaliar:

- necessidade real
- impacto
- simplicidade
- manutenção futura


Evitar:

- complexidade desnecessária
- microsserviços prematuros
- tecnologias apenas por moda


---

# Histórico do Projeto

Sempre manter arquivos:

/docs

contendo:

PROJECT_STATUS.md

Com:

- onde começou
- o que foi feito
- próximo passo
- decisões tomadas
- problemas encontrados


---

# Regras para IA

Ao continuar este projeto:

1. Não recriar estruturas já existentes.

2. Antes de alterar arquitetura analisar impacto.

3. Explicar decisões técnicas.

4. Priorizar código limpo.

5. Criar soluções simples primeiro.

6. Pensar em crescimento futuro.

7. Manter compatibilidade com evolução.


---

# Estado Atual

Projeto em fase de planejamento.

Já definido:

✔ Backend .NET C#
✔ SQL Server
✔ Arquitetura em camadas
✔ MVP focado em salgados
✔ Fluxo de pedidos
✔ Controle de produção


Próximas etapas:

1. Definir entidades
2. Criar banco
3. Criar estrutura da solução
4. Criar CRUD produtos
5. Criar fluxo de pedidos
6. Criar dashboard operacional