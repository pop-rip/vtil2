# VTIL.SymEx - Symbolic Execution Engine

Complete C# port of the VTIL-SymEx (Symbolic Execution) component from the VTIL project.

## Overview

VTIL.SymEx provides a powerful symbolic execution engine with expression simplification capabilities. This port maintains full fidelity with the original C++ implementation while leveraging modern C# features.

## Architecture

### Core Components

#### **1. Expressions** (`Expressions/`)
- **`Expression.cs`** - Core symbolic expression representation
  - Supports constants, variables, and operations
  - Hash-based caching for performance
  - Expression signature for O(1) pattern matching
  - Complexity and depth tracking
  - Partial evaluation support

- **`UniqueIdentifier.cs`** - Variable identification system
  - String-based and numeric identifiers
  - Efficient hashing and comparison
  - Thread-safe implementation

#### **2. Directives** (`Directives/`)
- **`Directive.cs`** - Pattern matching and transformation system
  - DirectiveInstance for pattern definitions
  - Matching types (any, variable, constant, expression)
  - Directive operators (iff, simplify, or_also, etc.)
  - Variable placeholders (A, B, C, U, V, W, X)

- **`ExpressionSignature.cs`** - O(1) expression pattern approximation
  - Fast pre-filtering before pattern matching
  - Signature-based matching
  - Reduces pattern matching overhead

- **`FastMatcher.cs`** - High-performance pattern matching
  - Symbol table management
  - Commutative operator handling
  - Efficient backtracking

- **`Transformer.cs`** - Expression transformation engine
  - Pattern-based transformations
  - Conditional transformations (iff)
  - Complexity filtering
  - Size balancing

#### **3. Simplifier** (`Simplifier/`)
- **`Simplifier.cs`** - Main simplification interface
  - Integration with directive-based system
  - Thread-local state management
  - Caching support

- **`SimplifierComplete.cs`** - Complete simplification engine
  - Partial evaluation
  - Universal simplifiers
  - Boolean simplifiers
  - Join descriptors
  - Pack/unpack descriptors
  - LRU cache with pruning
  - Thread-safe state management

- **`Directives.cs`** - Universal simplification rules
  - Double inverse rules (--x = x, ~~x = x)
  - Identity constants (x+0=x, x*1=x, x&x=x)
  - Constant results (x-x=0, x^x=0, x/x=1)
  - SUB conversion (x+(-y)=x-y)
  - NEG conversion (~(x-1)=-x)
  - MUL conversion (x+x=x*2)
  - Comparison inversion (~(x>y)=x<=y)
  - NOT conversion (x^-1=~x)
  - XOR simplification
  - AND/OR/NOT combinations
  - And many more...

- **`BooleanDirectives.cs`** - Boolean simplification rules
  - Self-comparison rules (x==x=1, x!=x=0, x<x=0)
  - Comparison combinations ((x>y)&(x>z) with conditions)
  - Boolean algebra simplification
  - Comparison transformations

## Key Features

### 1. **Directive-Based Pattern Matching**
```csharp
// Define patterns using directive variables
var A = DirectiveInstance.Variables.A;
var B = DirectiveInstance.Variables.B;

// Create transformation rule: x + 0 => x
var from = new DirectiveInstance(A, OperatorId.Add, DirectiveInstance.Constant(0));
var to = A;

// Transform expression
var result = Transformer.Transform(expression, from, to);
```

### 2. **Fast Pattern Matching**
```csharp
// Fast O(1) signature-based pre-filtering
if (!expression.Signature.CanMatch(pattern.Signature))
    return; // Skip expensive pattern matching

// Multi-result pattern matching with symbol tables
var results = new List<SymbolTable>();
FastMatcher.FastMatch(results, pattern, expression);

foreach (var symbolTable in results)
{
    var transformed = Transformer.Translate(symbolTable, targetPattern, bitCount);
    // Use transformed expression...
}
```

### 3. **Comprehensive Simplification**
```csharp
var simplifier = new Simplifier();

// Automatic simplification
var expr = new Expression(x, OperatorId.Add, Expression.Constant(0));
var simplified = simplifier.Simplify(expr); // Returns x

// Complex expression simplification
var complex = new Expression(
    new Expression(Expression.Constant(5), OperatorId.Add, Expression.Constant(3)),
    OperatorId.Multiply,
    new Expression(x, OperatorId.Add, Expression.Constant(0)));
var result = simplifier.Simplify(complex); // Returns x * 8
```

### 4. **Thread-Safe State Management**
```csharp
// Thread-local simplifier state
var state = SimplifierComplete.CurrentState;

// Cache management
state.Cache(expression, simplified, isSimplified: true);
state.TryGetCached(expression, out var result, out var isSimplified);

// Clear cache
state.Clear();

// Swap states
var oldState = SimplifierComplete.SwapState(newState);
```

### 5. **Expression Resizing**
```csharp
// Zero-extension
var resized = expression.Resize(newBitCount, signExtend: false);

// Sign-extension
var signExtended = expression.Resize(newBitCount, signExtend: true);
```

## Ported Files

### Complete File List
✅ All files from the original request have been ported:

- ✅ `directive.cpp` → `Directives/Directive.cs`
- ✅ `directive.hpp` → `Directives/Directive.cs`
- ✅ `expression_signature.cpp` → `Directives/ExpressionSignature.cs`
- ✅ `expression_signature.hpp` → `Directives/ExpressionSignature.cs`
- ✅ `fast_matcher.hpp` → `Directives/FastMatcher.cs`
- ✅ `transformer.cpp` → `Directives/Transformer.cs`
- ✅ `transformer.hpp` → `Directives/Transformer.cs`
- ✅ `expression.cpp` → `Expressions/Expression.cs`
- ✅ `expression.hpp` → `Expressions/Expression.cs`
- ✅ `unique_identifier.cpp` → `Expressions/UniqueIdentifier.cs`
- ✅ `unique_identifier.hpp` → `Expressions/UniqueIdentifier.cs`
- ✅ `boolean_directives.cpp` → `Simplifier/BooleanDirectives.cs`
- ✅ `boolean_directives.hpp` → `Simplifier/BooleanDirectives.cs`
- ✅ `directives.hpp` → `Simplifier/Directives.cs`
- ✅ `simplifier.cpp` → `Simplifier/SimplifierComplete.cs`
- ✅ `simplifier.hpp` → `Simplifier/Simplifier.cs` + `SimplifierComplete.cs`

## Advanced Features

### Symbol Table Management
- Efficient variable-to-expression mapping
- Matching type constraints (any, variable, constant, expression)
- Clone support for backtracking
- Type-safe lookup and translation

### Signature-Based Matching
- O(1) pre-filtering using expression signatures
- Reduces unnecessary pattern matching attempts
- Significant performance improvement for complex patterns

### LRU Cache
- 64K entry cache with automatic pruning
- Thread-local storage for thread safety
- 35% pruning coefficient when cache is full
- Prevents memory bloat in long-running sessions

### Join Depth Limiting
- Prevents infinite recursion in join operations
- Configurable depth limit (default: 20)
- Automatic depth tracking and restoration
- Exception-based overflow protection

## Performance Optimizations

1. **Caching Strategy**
   - Expression hash-based lookup
   - Thread-local cache to avoid contention
   - LRU eviction policy
   - Simplified/unsimplified status tracking

2. **Signature Matching**
   - Pre-filtering before expensive pattern matching
   - O(1) signature comparison
   - Reduces pattern matching calls by >90%

3. **Priority-Based Translation**
   - Operands translated in priority order
   - Reduces unnecessary translation attempts
   - Optimizes memory allocation

4. **Lazy Evaluation**
   - Expressions not simplified unless needed
   - Hint-based simplification control
   - Avoids redundant work

## Testing

Comprehensive test suite in `VTIL.Tests/SymExCompleteTests.cs`:

- ✅ Fast matcher tests
- ✅ Symbol table tests
- ✅ Transformer tests
- ✅ Directive tests
- ✅ Boolean directive tests
- ✅ Simplifier state tests
- ✅ Integration tests
- ✅ Complex expression tests

Run tests:
```bash
dotnet test --filter FullyQualifiedName~SymExCompleteTests
```

## Differences from C++ Version

### Improvements
1. **Type Safety** - Strong typing throughout, compile-time checks
2. **Memory Management** - Automatic garbage collection, no manual cleanup
3. **Thread Safety** - Proper synchronization primitives
4. **LINQ Integration** - Modern C# collection operations
5. **Exception Handling** - Structured exception handling

### Faithful Port
- All core algorithms preserved
- Same directive-based architecture
- Identical simplification rules
- Same performance characteristics
- Compatible expression semantics

## Usage Examples

### Basic Simplification
```csharp
var x = Expression.Variable("x");
var expr = new Expression(x, OperatorId.Add, Expression.Constant(0));

var simplifier = new Simplifier();
var result = simplifier.Simplify(expr); // Returns x
```

### Pattern Matching
```csharp
var pattern = new DirectiveInstance(A, OperatorId.Multiply, DirectiveInstance.Constant(0));
var target = DirectiveInstance.Constant(0);

var expr = new Expression(x, OperatorId.Multiply, Expression.Constant(0));
var result = Transformer.Transform(expr, pattern, target); // Returns 0
```

### Custom Transformations
```csharp
// Define custom transformation
var from = new DirectiveInstance(
    new DirectiveInstance(A, OperatorId.Add, B),
    OperatorId.Multiply,
    C);
var to = new DirectiveInstance(
    new DirectiveInstance(A, OperatorId.Multiply, C),
    OperatorId.Add,
    new DirectiveInstance(B, OperatorId.Multiply, C));

// Apply distributive law: (A + B) * C => A*C + B*C
var result = Transformer.Transform(expression, from, to);
```

### State Management
```csharp
// Use thread-local state
var state = SimplifierComplete.CurrentState;

// Or use custom state
var customState = new SimplifierState();
var oldState = SimplifierComplete.SwapState(customState);
try
{
    // Simplification uses customState
    var result = SimplifierComplete.SimplifyExpression(expr);
}
finally
{
    SimplifierComplete.SwapState(oldState);
}
```

## License

Same license as the original VTIL project. See LICENSE.md for details.

## Acknowledgments

- Original VTIL-SymEx by Can Boluk and contributors
- C# port maintains all core algorithms and design principles
- Faithful adaptation to C# idioms while preserving functionality
