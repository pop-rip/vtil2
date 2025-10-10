/*
 * Copyright (c) 2020 pop-rip and the contributors of the VTIL2 Project   
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
using System.Collections.Generic;
using System.Linq;
using VTIL.Architecture;
using VTIL.Common.Math;
using VTIL.SymEx;

namespace VTIL.Compiler.Optimizer
{
    /// <summary>
    /// Rewrites instructions using symbolic execution to find better implementations.
    /// </summary>
    public class SymbolicRewritePass : OptimizationPassBase
    {
        private readonly object _lockObject = new object();
        private readonly bool _allowCrossBlock;

        /// <summary>
        /// Gets the execution order for this pass (parallel).
        /// </summary>
        public override ExecutionOrder ExecutionOrder => ExecutionOrder.Parallel;

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public override string Name => "Symbolic Rewrite";

        /// <summary>
        /// Creates a new symbolic rewrite pass.
        /// </summary>
        public SymbolicRewritePass(bool allowCrossBlock = true)
        {
            _allowCrossBlock = allowCrossBlock;
        }

        /// <summary>
        /// Passes a single basic block through the optimizer.
        /// </summary>
        public override int Pass(BasicBlock block, bool crossBlock = false)
        {
            lock (_lockObject)
            {
                return RewriteInstructions(block, crossBlock);
            }
        }

        /// <summary>
        /// Rewrites instructions using symbolic execution.
        /// </summary>
        private int RewriteInstructions(BasicBlock block, bool crossBlock)
        {
            var rewrittenCount = 0;
            var simplifier = new Simplifier();

            for (int i = 0; i < block.InstructionCount; i++)
            {
                var instruction = block.GetInstruction(i);
                var rewritten = RewriteInstruction(instruction, simplifier, block, i);

                if (rewritten != null && !rewritten.Equals(instruction))
                {
                    block.ReplaceInstruction(i, rewritten);
                    rewrittenCount++;
                }
            }

            return rewrittenCount;
        }

        /// <summary>
        /// Rewrites a single instruction using symbolic execution.
        /// </summary>
        private Instruction? RewriteInstruction(Instruction instruction, Simplifier simplifier, BasicBlock block, int instructionIndex)
        {
            // Convert instruction to symbolic expression
            var expression = InstructionToExpression(instruction);
            if (expression == null)
                return null;

            // Simplify the expression
            var simplified = simplifier.Simplify(expression);

            // Check if simplification resulted in a better instruction sequence
            var newInstructions = ExpressionToInstructions(simplified, instruction);
            if (newInstructions != null && newInstructions.Count == 1)
            {
                return newInstructions[0];
            }

            // If we can't simplify to a single instruction, try pattern-based rewriting
            return ApplyPatternRewrites(instruction, block, instructionIndex);
        }

        /// <summary>
        /// Converts an instruction to a symbolic expression.
        /// </summary>
        private Expression? InstructionToExpression(Instruction instruction)
        {
            switch (instruction.Descriptor)
            {
                case var desc when desc == InstructionSet.Add:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.Add, src2);
                        }
                    }
                    break;

                case var desc when desc == InstructionSet.Sub:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.Subtract, src2);
                        }
                    }
                    break;

                case var desc when desc == InstructionSet.Mul:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.Multiply, src2);
                        }
                    }
                    break;

                case var desc when desc == InstructionSet.Div:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.Divide, src2);
                        }
                    }
                    break;

                case var desc when desc == InstructionSet.And:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.BitwiseAnd, src2);
                        }
                    }
                    break;

                case var desc when desc == InstructionSet.Or:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.BitwiseOr, src2);
                        }
                    }
                    break;

                case var desc when desc == InstructionSet.Xor:
                    if (instruction.OperandCount >= 3)
                    {
                        var dest = OperandToExpression(instruction.GetOperand0());
                        var src1 = OperandToExpression(instruction.GetOperand1());
                        var src2 = OperandToExpression(instruction.GetOperand2());
                        
                        if (dest != null && src1 != null && src2 != null)
                        {
                            return new Expression(src1, OperatorId.BitwiseXor, src2);
                        }
                    }
                    break;
            }

            return null;
        }

        /// <summary>
        /// Converts an operand to a symbolic expression.
        /// </summary>
        private Expression? OperandToExpression(Operand operand)
        {
            if (operand.IsImmediate)
            {
                return Expression.Constant(operand.GetImmediate());
            }
            else if (operand.IsRegister)
            {
                var register = operand.GetRegister();
                return Expression.Variable($"reg_{register.Id}_{register.SizeInBits}");
            }

            return null;
        }

        /// <summary>
        /// Converts a simplified expression back to instructions.
        /// </summary>
        private List<Instruction>? ExpressionToInstructions(Expression expression, Instruction originalInstruction)
        {
            if (expression.IsConstant)
            {
                // Constant result - replace with immediate load
                var constant = expression.GetConstantValue();
                var immediate = Operand.CreateImmediate(constant, originalInstruction.AccessSize);
                var dest = originalInstruction.GetOperand0();
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                };
            }
            else if (expression.IsVariable)
            {
                // Variable result - replace with register move
                var varName = expression.GetVariableName();
                var dest = originalInstruction.GetOperand0();
                
                // This would need more sophisticated register mapping
                return null;
            }
            else if (expression.IsOperation)
            {
                // Operation result - check if it can be simplified
                return SimplifyOperation(expression, originalInstruction);
            }

            return null;
        }

        /// <summary>
        /// Simplifies an operation expression.
        /// </summary>
        private List<Instruction>? SimplifyOperation(Expression expression, Instruction originalInstruction)
        {
            // Check for common simplifications
            switch (expression.Operator)
            {
                case OperatorId.Add:
                    return SimplifyAdd(expression, originalInstruction);
                case OperatorId.Multiply:
                    return SimplifyMultiply(expression, originalInstruction);
                case OperatorId.BitwiseAnd:
                    return SimplifyBitwiseAnd(expression, originalInstruction);
                case OperatorId.BitwiseOr:
                    return SimplifyBitwiseOr(expression, originalInstruction);
            }

            return null;
        }

        /// <summary>
        /// Simplifies an addition operation.
        /// </summary>
        private List<Instruction>? SimplifyAdd(Expression expression, Instruction originalInstruction)
        {
            if (expression.Lhs!.IsConstant && expression.Rhs!.IsConstant)
            {
                var result = expression.Lhs.GetConstantValue() + expression.Rhs.GetConstantValue();
                var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                var dest = originalInstruction.GetOperand0();
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                };
            }
            else if (expression.Lhs.IsConstant && expression.Lhs.GetConstantValue() == 0)
            {
                // x + 0 = x
                var dest = originalInstruction.GetOperand0();
                var src = Operand.CreateReadRegister(GetRegisterFromExpression(expression.Rhs!), GetRegisterFromExpression(expression.Rhs!).SizeInBits);
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }
            else if (expression.Rhs.IsConstant && expression.Rhs.GetConstantValue() == 0)
            {
                // 0 + x = x
                var dest = originalInstruction.GetOperand0();
                var src = Operand.CreateReadRegister(GetRegisterFromExpression(expression.Lhs!), GetRegisterFromExpression(expression.Lhs!).SizeInBits);
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }

            return null;
        }

        /// <summary>
        /// Simplifies a multiplication operation.
        /// </summary>
        private List<Instruction>? SimplifyMultiply(Expression expression, Instruction originalInstruction)
        {
            if (expression.Lhs!.IsConstant && expression.Rhs!.IsConstant)
            {
                var result = expression.Lhs.GetConstantValue() * expression.Rhs.GetConstantValue();
                var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                var dest = originalInstruction.GetOperand0();
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                };
            }
            else if (expression.Lhs.IsConstant && expression.Lhs.GetConstantValue() == 1)
            {
                // x * 1 = x
                var dest = originalInstruction.GetOperand0();
                var src = Operand.CreateReadRegister(GetRegisterFromExpression(expression.Rhs!), GetRegisterFromExpression(expression.Rhs!).SizeInBits);
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }
            else if (expression.Rhs.IsConstant && expression.Rhs.GetConstantValue() == 1)
            {
                // 1 * x = x
                var dest = originalInstruction.GetOperand0();
                var src = Operand.CreateReadRegister(GetRegisterFromExpression(expression.Lhs!), GetRegisterFromExpression(expression.Lhs!).SizeInBits);
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }

            return null;
        }

        /// <summary>
        /// Simplifies a bitwise AND operation.
        /// </summary>
        private List<Instruction>? SimplifyBitwiseAnd(Expression expression, Instruction originalInstruction)
        {
            if (expression.Lhs!.IsConstant && expression.Rhs!.IsConstant)
            {
                var result = expression.Lhs.GetConstantValue() & expression.Rhs.GetConstantValue();
                var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                var dest = originalInstruction.GetOperand0();
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                };
            }
            else if (AreExpressionsEqual(expression.Lhs, expression.Rhs))
            {
                // x & x = x
                var dest = originalInstruction.GetOperand0();
                var src = Operand.CreateReadRegister(GetRegisterFromExpression(expression.Lhs), GetRegisterFromExpression(expression.Lhs).SizeInBits);
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }

            return null;
        }

        /// <summary>
        /// Simplifies a bitwise OR operation.
        /// </summary>
        private List<Instruction>? SimplifyBitwiseOr(Expression expression, Instruction originalInstruction)
        {
            if (expression.Lhs!.IsConstant && expression.Rhs!.IsConstant)
            {
                var result = expression.Lhs.GetConstantValue() | expression.Rhs.GetConstantValue();
                var immediate = Operand.CreateImmediate(result, originalInstruction.AccessSize);
                var dest = originalInstruction.GetOperand0();
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, immediate, immediate.SizeInBits)
                };
            }
            else if (AreExpressionsEqual(expression.Lhs, expression.Rhs))
            {
                // x | x = x
                var dest = originalInstruction.GetOperand0();
                var src = Operand.CreateReadRegister(GetRegisterFromExpression(expression.Lhs), GetRegisterFromExpression(expression.Lhs).SizeInBits);
                
                return new List<Instruction>
                {
                    Instruction.CreateMov(dest, src, src.SizeInBits)
                };
            }

            return null;
        }

        /// <summary>
        /// Applies pattern-based rewrites to an instruction.
        /// </summary>
        private Instruction? ApplyPatternRewrites(Instruction instruction, BasicBlock block, int instructionIndex)
        {
            // This would contain pattern-based rewrites that can't be easily expressed
            // through symbolic execution, such as instruction fusion, strength reduction, etc.
            
            return null;
        }

        /// <summary>
        /// Gets a register from an expression (simplified implementation).
        /// </summary>
        private RegisterDescriptor GetRegisterFromExpression(Expression expression)
        {
            // This is a simplified implementation - in practice, we'd need
            // a more sophisticated mapping from expressions back to registers
            return RegisterDescriptor.CreateInternal(0, expression.Depth);
        }

        /// <summary>
        /// Checks if two expressions are equal.
        /// </summary>
        private bool AreExpressionsEqual(Expression expr1, Expression expr2)
        {
            if (expr1.IsConstant && expr2.IsConstant)
                return expr1.GetConstantValue() == expr2.GetConstantValue();
            
            if (expr1.IsVariable && expr2.IsVariable)
                return expr1.GetVariableName() == expr2.GetVariableName();
            
            if (expr1.IsOperation && expr2.IsOperation)
                return expr1.Operator == expr2.Operator && 
                       AreExpressionsEqual(expr1.Lhs!, expr2.Lhs!) && 
                       AreExpressionsEqual(expr1.Rhs!, expr2.Rhs!);
            
            return false;
        }
    }
}
