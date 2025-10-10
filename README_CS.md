# VTIL Core - C# Port

This is a complete port of the VTIL (Virtual-machine Translation Intermediate Language) project from C++ to C#. VTIL is designed for binary de-obfuscation and de-virtualization, providing an optimizing compiler with an extremely versatile intermediate language.

## Features

- **Clean C# Architecture**: Modern C# design with proper namespaces and assemblies
- **Symbolic Execution Engine**: Complete symbolic expression system with simplification
- **Architecture Support**: Support for x86, AMD64, ARM64, and virtual architectures
- **Instruction Set**: Comprehensive VTIL instruction set with operands and descriptors
- **Basic Block Management**: Control flow graph representation with blocks and connections
- **Expression Simplification**: Advanced simplification rules for symbolic expressions
- **Pattern Matching**: Directive system for expression pattern matching and transformation

## Project Structure

```
VTIL.Core.csproj           # Main project file
├── VTIL.Common/           # Core utilities and base types
│   ├── IO/               # Logging, formatting, and I/O utilities
│   ├── Math/             # Mathematical operations and operators
│   └── Util/             # Utility classes and helpers
├── VTIL.Architecture/     # Core architecture definitions
│   ├── Arch/             # Architecture types, instructions, operands
│   └── Routine/          # Routines, basic blocks, call conventions
├── VTIL.SymEx/           # Symbolic execution engine
│   ├── Expressions/      # Symbolic expressions and identifiers
│   ├── Directives/       # Pattern matching and directives
│   └── Simplifier/       # Expression simplification rules
├── VTIL.Compiler/        # Optimization passes (placeholder)
├── VTIL.Tests/           # Comprehensive test suite
└── Program.cs            # Main entry point with demos
```

## Quick Start

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension

### Building

```bash
# Clone the repository
git clone <repository-url>
cd vtil2

# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the demo
dotnet run --project VTIL.Core -- demo
```

### Running Demos

The main program provides several demonstration modes:

```bash
# Run basic demo
dotnet run --project VTIL.Core -- demo

# Run tests
dotnet run --project VTIL.Core -- test

# Run simplifier demo
dotnet run --project VTIL.Core -- simplify

# Show help
dotnet run --project VTIL.Core -- help
```

## Core Components

### 1. Architecture System

```csharp
// Create a routine for AMD64 architecture
var routine = new Routine(ArchitectureIdentifier.Amd64);

// Allocate registers
var reg1 = routine.AllocRegister(64);  // 64-bit register
var reg2 = routine.AllocRegister(32);  // 32-bit register

// Create operands
var imm = Operand.CreateImmediate(42, 64);
var regOp = Operand.CreateWriteRegister(reg1, 64);
```

### 2. Instruction System

```csharp
// Create instructions
var mov = Instruction.CreateMov(regOp, imm, 64);
var add = Instruction.CreateAdd(regOp, Operand.CreateReadRegister(reg2, 64), 64);

// Create basic blocks
var (block, _) = routine.CreateBlock(0x1000);
block.AddInstruction(mov);
block.AddInstruction(add);
```

### 3. Symbolic Expressions

```csharp
// Create symbolic expressions
var x = Expression.Variable("x");
var y = Expression.Constant(10);
var expr = new Expression(x, OperatorId.Add, y);

// Evaluate expressions
var result = expr.Evaluate();  // null if contains variables
var substituted = expr.Substitute("x", Expression.Constant(5));
var evaluated = substituted.Evaluate();  // 15
```

### 4. Expression Simplification

```csharp
var simplifier = new Simplifier();

// Simplify expressions
var expr = new Expression(x, OperatorId.Add, Expression.Constant(0));
var simplified = simplifier.Simplify(expr);  // Returns x

// Complex simplification
var complex = new Expression(
    new Expression(Expression.Constant(5), OperatorId.Add, Expression.Constant(3)),
    OperatorId.Multiply,
    new Expression(x, OperatorId.Add, Expression.Constant(0)));
var result = simplifier.Simplify(complex);  // Returns x * 8
```

### 5. Pattern Matching

```csharp
// Create directive patterns
var pattern = new DirectiveInstance(
    DirectiveInstance.Variables.A,  // Match any expression
    OperatorId.Add,
    DirectiveInstance.Variables.U); // Match constant

// Match against expressions
var bindings = new Dictionary<string, Expression>();
if (pattern.TryMatch(expr, bindings))
{
    var matchedA = bindings["α"];
    var matchedU = bindings["Σ"];
}
```

## Key Features

### Symbolic Execution
- Complete symbolic expression system
- Variable substitution and evaluation
- Expression complexity tracking
- Hash-based caching for performance

### Expression Simplification
- Constant folding
- Identity rules (x + 0 = x, x * 1 = x)
- Zero rules (x * 0 = 0)
- Bitwise rules (x & x = x, x ^ x = 0)
- Algebraic rules (associative, commutative, distributive)

### Architecture Support
- Multiple architecture identifiers (x86, AMD64, ARM64, Virtual)
- Register descriptors with type and size information
- Operand system supporting immediate values and registers
- Instruction descriptors with operand types and semantics

### Control Flow
- Basic block representation
- Control flow graph with predecessors and successors
- Path finding and reachability analysis
- Loop detection

### Pattern Matching
- Directive system for expression patterns
- Variable matching with constraints
- Binding extraction from matched patterns
- Support for complex nested patterns

## Testing

The project includes comprehensive tests covering:

- Architecture identifier functionality
- Register and operand systems
- Instruction creation and manipulation
- Basic block and routine management
- Symbolic expression evaluation
- Expression simplification rules
- Pattern matching and directives
- Memory management and cloning

Run tests with:
```bash
dotnet test
```

## Performance Considerations

- **Expression Caching**: Simplifier uses hash-based caching to avoid redundant work
- **Immutable Design**: Core types are immutable where possible for thread safety
- **Efficient Hashing**: Custom hash functions for fast expression comparison
- **Lazy Evaluation**: Support for lazy expression evaluation where appropriate

## Differences from C++ Version

1. **Memory Management**: Uses .NET garbage collection instead of manual memory management
2. **Type Safety**: Strong typing throughout with compile-time checks
3. **Exception Handling**: Uses C# exceptions instead of C++ error codes
4. **Collections**: Uses .NET collections (List, Dictionary) instead of STL containers
5. **String Handling**: Uses C# strings with proper Unicode support
6. **Threading**: Uses .NET threading primitives and atomic operations

## Contributing

This is a port of the original VTIL project. For the original C++ version, see the [VTIL Project](https://github.com/vtil-project/VTIL-Core).

## License

Same license as the original VTIL project. See LICENSE.md for details.

## Acknowledgments

- Original VTIL project by Can Boluk and contributors
- C# port maintains the same architectural principles and design philosophy
- All core algorithms and data structures preserved from the original implementation
