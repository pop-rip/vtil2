/*
 * Copyright (c) 2020 Can Boluk and contributors of the VTIL Project   
 * All rights reserved.   
 *    
 * Redistribution and use in source and binary forms, with or without   
 * modification, are permitted provided that the following conditions are met: 
 *    
 * 1. Redistributions of source code must retain the above copyright notice,   
 *    this list of conditions and the following disclaimer.   
 * 2. Redistributions in binary form must reproduce the above copyright   
 *    notice, this list of conditions and the following disclaimer in the   
 *    documentation and/or other materials provided with the distribution.   
 * 3. Neither the name of VTIL Project nor the names of its contributors
 *    may be used to endorse or promote products derived from this software 
 *    without specific prior written permission.   
 *    
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE   
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE  
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE   
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR   
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF   
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS   
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN   
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)   
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE  
 * POSSIBILITY OF SUCH DAMAGE.        
 */

using System;
using System.Numerics;
using Xunit;
using VTIL.Architecture;
using VTIL.Common.Math;
using VTIL.SymEx;

namespace VTIL.Tests
{
    /// <summary>
    /// Comprehensive tests for VTIL Core functionality.
    /// </summary>
    public class VTILTests
    {
        [Fact]
        public void TestArchitectureIdentifier()
        {
            var arch = ArchitectureIdentifier.Amd64;
            Assert.Equal("amd64", arch.GetName());
            Assert.Equal(64, arch.GetPointerSize());
            Assert.True(arch.Is64Bit());
            Assert.False(arch.Is32Bit());
        }

        [Fact]
        public void TestVipT()
        {
            var vip1 = new VipT(0x1000);
            var vip2 = new VipT(0x2000);
            var vip3 = vip1 + 0x1000;

            Assert.Equal(0x1000UL, vip1.Value);
            Assert.Equal(0x2000UL, vip2.Value);
            Assert.Equal(vip2.Value, vip3.Value);
            Assert.True(vip1 < vip2);
            Assert.True(vip2 > vip1);
        }

        [Fact]
        public void TestBitCntT()
        {
            var bits1 = new BitCntT(64);
            var bits2 = new BitCntT(32);
            var bits3 = bits1 - bits2;

            Assert.Equal(64, bits1.Value);
            Assert.Equal(32, bits2.Value);
            Assert.Equal(32, bits3.Value);
            Assert.True(bits1 > bits2);
        }

        [Fact]
        public void TestRegisterDescriptor()
        {
            var reg1 = RegisterDescriptor.CreateInternal(1, 64);
            var reg2 = RegisterDescriptor.CreateGeneralPurpose(0, 32);

            Assert.True(reg1.IsInternal);
            Assert.False(reg1.IsGeneralPurpose);
            Assert.Equal(64, reg1.SizeInBits);
            Assert.Equal(8, reg1.SizeInBytes);
            Assert.Equal("v1", reg1.GetName());
        }

        [Fact]
        public void TestOperand()
        {
            var reg = RegisterDescriptor.CreateInternal(1, 64);
            var imm = Operand.CreateImmediate(42, 64);
            var regOp = Operand.CreateWriteRegister(reg, 64);

            Assert.True(imm.IsImmediate);
            Assert.False(imm.IsRegister);
            Assert.True(regOp.IsRegister);
            Assert.False(regOp.IsImmediate);
            Assert.True(regOp.IsWritable);
            Assert.False(imm.IsWritable);
        }

        [Fact]
        public void TestInstructionDescriptor()
        {
            var mov = InstructionSet.Mov;
            Assert.Equal("mov", mov.Name);
            Assert.Equal(2, mov.OperandCount);
            Assert.False(mov.IsBranching);
            Assert.False(mov.IsVolatile);
            Assert.Equal(OperatorId.Cast, mov.SymbolicOperator);
        }

        [Fact]
        public void TestInstruction()
        {
            var reg = RegisterDescriptor.CreateInternal(1, 64);
            var imm = Operand.CreateImmediate(123, 64);
            var regOp = Operand.CreateWriteRegister(reg, 64);
            var mov = Instruction.CreateMov(regOp, imm, 64);

            Assert.Equal(InstructionSet.Mov, mov.Descriptor);
            Assert.Equal(2, mov.OperandCount);
            Assert.Equal(regOp, mov.GetOperand0());
            Assert.Equal(imm, mov.GetOperand1());
            Assert.Equal(64, mov.AccessSize.Value);
        }

        [Fact]
        public void TestBasicBlock()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            var (block, created) = routine.CreateBlock(0x1000);

            Assert.True(created);
            Assert.Equal(0x1000UL, block.Vip.Value);
            Assert.Equal(0, block.InstructionCount);
            Assert.True(block.IsEntry);
            Assert.False(block.IsExit);
        }

        [Fact]
        public void TestRoutine()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            Assert.Equal(ArchitectureIdentifier.Amd64, routine.ArchitectureId);
            Assert.Equal(0, routine.BlockCount);
            Assert.Equal(0, routine.InstructionCount);

            // Allocate registers
            var reg1 = routine.AllocRegister(64);
            var reg2 = routine.AllocRegister(32);
            Assert.NotEqual(reg1.Id, reg2.Id);

            // Create blocks
            var (block1, _) = routine.CreateBlock(0x1000);
            var (block2, _) = routine.CreateBlock(0x2000);
            Assert.Equal(2, routine.BlockCount);
            Assert.Equal(block1, routine.EntryPoint);
        }

        [Fact]
        public void TestCallConvention()
        {
            var convention = new CallConvention(
                purgeStack: true,
                stackPurgeBytes: 8,
                stackAlignment: 16,
                shadowSpace: 32);

            Assert.True(convention.PurgeStack);
            Assert.Equal(8, convention.StackPurgeBytes);
            Assert.Equal(16, convention.StackAlignment);
            Assert.Equal(32, convention.ShadowSpace);
        }

        [Fact]
        public void TestUniqueIdentifier()
        {
            var uid1 = new UniqueIdentifier("test_var");
            var uid2 = new UniqueIdentifier("test_var");
            var uid3 = new UniqueIdentifier("different_var");

            Assert.Equal(uid1, uid2);
            Assert.NotEqual(uid1, uid3);
            Assert.Equal("test_var", uid1.ToString());
            Assert.True(uid1.IsString);
            Assert.False(uid1.IsNumeric);
        }

        [Fact]
        public void TestUniqueIdentifierNumeric()
        {
            var uid1 = new UniqueIdentifier(123);
            var uid2 = new UniqueIdentifier(123);
            var uid3 = new UniqueIdentifier(456);

            Assert.Equal(uid1, uid2);
            Assert.NotEqual(uid1, uid3);
            Assert.True(uid1.IsNumeric);
            Assert.False(uid1.IsString);
            Assert.Equal(123, uid1.GetNumericValue());
        }

        [Fact]
        public void TestExpressionSignature()
        {
            var sig1 = new ExpressionSignature((BigInteger)42);
            var sig2 = new ExpressionSignature((BigInteger)42);
            var sig3 = new ExpressionSignature((BigInteger)123);

            Assert.Equal(sig1, sig2);
            Assert.NotEqual(sig1, sig3);
            Assert.True(sig1.IsConstant());
        }

        [Fact]
        public void TestExpressionConstant()
        {
            var expr = Expression.Constant(42);
            Assert.True(expr.IsConstant);
            Assert.False(expr.IsVariable);
            Assert.False(expr.IsOperation);
            Assert.Equal(42, expr.GetConstantValue());
            Assert.Equal("42", expr.ToString());
        }

        [Fact]
        public void TestExpressionVariable()
        {
            var expr = Expression.Variable("x");
            Assert.False(expr.IsConstant);
            Assert.True(expr.IsVariable);
            Assert.False(expr.IsOperation);
            Assert.Equal("x", expr.GetVariableName());
            Assert.Equal("x", expr.ToString());
        }

        [Fact]
        public void TestExpressionOperation()
        {
            var x = Expression.Variable("x");
            var y = Expression.Constant(10);
            var add = new Expression(x, OperatorId.Add, y);

            Assert.False(add.IsConstant);
            Assert.False(add.IsVariable);
            Assert.True(add.IsOperation);
            Assert.True(add.IsBinaryOperation);
            Assert.Equal(OperatorId.Add, add.Operator);
            Assert.Equal(x, add.Lhs);
            Assert.Equal(y, add.Rhs);
            Assert.Equal("(x + 10)", add.ToString());
        }

        [Fact]
        public void TestExpressionEvaluation()
        {
            var expr1 = Expression.Constant(10);
            var expr2 = Expression.Constant(20);
            var add = new Expression(expr1, OperatorId.Add, expr2);
            var result = add.Evaluate();

            Assert.Equal(30, result);
        }

        [Fact]
        public void TestExpressionSubstitution()
        {
            var x = Expression.Variable("x");
            var y = Expression.Constant(5);
            var expr = new Expression(x, OperatorId.Add, Expression.Constant(10));
            var substituted = expr.Substitute("x", y);

            var result = substituted.Evaluate();
            Assert.Equal(15, result);
        }

        [Fact]
        public void TestExpressionContainsVariable()
        {
            var x = Expression.Variable("x");
            var y = Expression.Variable("y");
            var expr = new Expression(x, OperatorId.Add, y);

            Assert.True(expr.ContainsVariable("x"));
            Assert.True(expr.ContainsVariable("y"));
            Assert.False(expr.ContainsVariable("z"));
            Assert.True(expr.ContainsVariables());
        }

        [Fact]
        public void TestDirectiveInstance()
        {
            var varA = DirectiveInstance.Variables.A;
            var varB = DirectiveInstance.Variables.B;
            var x = Expression.Variable("x");
            var y = Expression.Constant(5);

            Assert.Equal("α", varA.Id);
            Assert.True(varA.IsVariable);
            Assert.False(varA.IsOperation);

            var bindings = new System.Collections.Generic.Dictionary<string, Expression>();
            Assert.True(varA.TryMatch(x, bindings));
            Assert.Equal(x, bindings["α"]);
        }

        [Fact]
        public void TestDirectiveMatching()
        {
            var varV = DirectiveInstance.Variables.V; // Match variable
            var varU = DirectiveInstance.Variables.U; // Match constant
            var x = Expression.Variable("x");
            var y = Expression.Constant(5);

            var bindings = new System.Collections.Generic.Dictionary<string, Expression>();
            Assert.True(varV.TryMatch(x, bindings));
            Assert.False(varV.TryMatch(y, bindings));
            Assert.True(varU.TryMatch(y, bindings));
            Assert.False(varU.TryMatch(x, bindings));
        }

        [Fact]
        public void TestSimplifierConstantFolding()
        {
            var simplifier = new Simplifier();
            var expr = new Expression(Expression.Constant(10), OperatorId.Add, Expression.Constant(20));
            var simplified = simplifier.Simplify(expr);

            Assert.True(simplified.IsConstant);
            Assert.Equal(30, simplified.GetConstantValue());
        }

        [Fact]
        public void TestSimplifierIdentityRule()
        {
            var simplifier = new Simplifier();
            var x = Expression.Variable("x");
            var expr = new Expression(x, OperatorId.Add, Expression.Constant(0));
            var simplified = simplifier.Simplify(expr);

            Assert.Equal(x, simplified);
        }

        [Fact]
        public void TestSimplifierZeroRule()
        {
            var simplifier = new Simplifier();
            var x = Expression.Variable("x");
            var expr = new Expression(x, OperatorId.Multiply, Expression.Constant(0));
            var simplified = simplifier.Simplify(expr);

            Assert.True(simplified.IsConstant);
            Assert.Equal(0, simplified.GetConstantValue());
        }

        [Fact]
        public void TestSimplifierBitwiseRule()
        {
            var simplifier = new Simplifier();
            var x = Expression.Variable("x");
            var expr = new Expression(x, OperatorId.BitwiseAnd, x);
            var simplified = simplifier.Simplify(expr);

            Assert.Equal(x, simplified);
        }

        [Fact]
        public void TestComplexExpressionSimplification()
        {
            var simplifier = new Simplifier();
            var x = Expression.Variable("x");
            
            // (x + 0) * (5 + 3) = x * 8
            var expr = new Expression(
                new Expression(x, OperatorId.Add, Expression.Constant(0)),
                OperatorId.Multiply,
                new Expression(Expression.Constant(5), OperatorId.Add, Expression.Constant(3)));

            var simplified = simplifier.Simplify(expr);

            // Should simplify to x * 8
            Assert.True(simplified.IsBinaryOperation);
            Assert.Equal(OperatorId.Multiply, simplified.Operator);
            Assert.Equal(x, simplified.Lhs);
            Assert.True(simplified.Rhs!.IsConstant);
            Assert.Equal(8, simplified.Rhs.GetConstantValue());
        }

        [Fact]
        public void TestRoutineBlockConnections()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            var (block1, _) = routine.CreateBlock(0x1000);
            var (block2, _) = routine.CreateBlock(0x2000);

            block1.AddSuccessor(block2);

            Assert.Equal(1, block1.Successors.Count);
            Assert.Equal(1, block2.Predecessors.Count);
            Assert.Equal(block2, block1.Successors[0]);
            Assert.Equal(block1, block2.Predecessors[0]);
            Assert.True(routine.HasPath(block1, block2));
        }

        [Fact]
        public void TestRoutineClone()
        {
            var routine = new Routine(ArchitectureIdentifier.Amd64);
            var reg = routine.AllocRegister(64);
            var (block, _) = routine.CreateBlock(0x1000);
            
            var mov = Instruction.CreateMov(
                Operand.CreateWriteRegister(reg, 64),
                Operand.CreateImmediate(42, 64),
                64);
            block.AddInstruction(mov);

            var clone = routine.Clone();

            Assert.Equal(routine.ArchitectureId, clone.ArchitectureId);
            Assert.Equal(routine.BlockCount, clone.BlockCount);
            Assert.Equal(routine.InstructionCount, clone.InstructionCount);
            Assert.NotEqual(routine, clone);
        }

        [Fact]
        public void TestCallConventionClone()
        {
            var original = new CallConvention(
                purgeStack: true,
                stackPurgeBytes: 8,
                stackAlignment: 16);

            var clone = original.Clone();

            Assert.Equal(original.PurgeStack, clone.PurgeStack);
            Assert.Equal(original.StackPurgeBytes, clone.StackPurgeBytes);
            Assert.Equal(original.StackAlignment, clone.StackAlignment);
            Assert.NotEqual(original, clone); // Different instances
        }

        [Fact]
        public void TestExpressionDepth()
        {
            var x = Expression.Variable("x");
            var y = Expression.Variable("y");
            var z = Expression.Variable("z");

            var expr1 = new Expression(x, OperatorId.Add, y);
            Assert.Equal(1, expr1.Depth);

            var expr2 = new Expression(expr1, OperatorId.Multiply, z);
            Assert.Equal(2, expr2.Depth);

            var expr3 = new Expression(expr2, OperatorId.Subtract, Expression.Constant(1));
            Assert.Equal(3, expr3.Depth);
        }

        [Fact]
        public void TestExpressionOperands()
        {
            var x = Expression.Variable("x");
            var y = Expression.Variable("y");
            var expr = new Expression(x, OperatorId.Add, y);

            var operands = expr.GetOperands();
            Assert.Equal(2, operands.Length);
            Assert.Equal(x, operands[0]);
            Assert.Equal(y, operands[1]);
            Assert.Equal(x, expr.GetOperand0());
            Assert.Equal(y, expr.GetOperand1());
        }

        [Fact]
        public void TestExpressionSignatureUnary()
        {
            var x = Expression.Variable("x");
            var expr = new Expression(OperatorId.Negate, x);
            var sig = expr.Signature;

            Assert.True(sig.IsUnaryOperation());
            Assert.False(sig.IsBinaryOperation());
            Assert.False(sig.IsConstant());
        }

        [Fact]
        public void TestExpressionSignatureBinary()
        {
            var x = Expression.Variable("x");
            var y = Expression.Variable("y");
            var expr = new Expression(x, OperatorId.Add, y);
            var sig = expr.Signature;

            Assert.True(sig.IsBinaryOperation());
            Assert.False(sig.IsUnaryOperation());
            Assert.False(sig.IsConstant());
        }
    }
}
