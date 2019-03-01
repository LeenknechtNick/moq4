// Copyright (c) 2007, Clarius Consulting, Manas Technology Solutions, InSTEDD.
// All rights reserved. Licensed under the BSD 3-Clause License; see License.txt.

using System;
using System.Linq.Expressions;

using Xunit;

namespace Moq.Tests
{
	public class ActionObserverFixture
	{
		public class Reconstructibility
		{
			// NOTE: These tests might look pointless at first glance, until you notice
			// the signature of `AssertReconstructable`: delegates are being compared to
			// LINQ expression trees for equality.

			[Fact]
			public void Void_method_call()
			{
				AssertReconstructable(
					x => x.Void(),
					x => x.Void());
			}

			[Fact]
			public void Void_method_call_with_arg()
			{
				AssertReconstructable(
					x => x.VoidWithInt(42),
					x => x.VoidWithInt(42));
			}

			[Fact]
			public void Void_method_call_with_coerced_arg()
			{
				AssertReconstructable(
					x => x.VoidWithLong(42),
					x => x.VoidWithLong(42));
			}

			[Fact]
			public void Void_method_call_with_coerced_nullable_arg()
			{
				AssertReconstructable(
					x => x.VoidWithNullableInt(42),
					x => x.VoidWithNullableInt(42));
			}

			[Fact]
			public void Void_method_call_with_cast_arg()
			{
				AssertReconstructable(
					x => x.VoidWithInt((int)42L),
					x => x.VoidWithInt((int)42L));
			}

			[Fact]
			public void Void_method_call_with_arg_evaluated()
			{
				int arg = 42;
				AssertReconstructable(
					x => x.VoidWithInt(42),
					x => x.VoidWithInt(arg));
			}

			[Fact]
			public void Void_method_call_on_sub_object()
			{
				AssertReconstructable(
					x => x.GetY().Z.Void(),
					x => x.GetY().Z.Void());
			}

			[Fact]
			public void Void_method_call_on_sub_with_several_args()
			{
				AssertReconstructable(
					x => x.GetY(1).Z.VoidWithIntInt(2, 3),
					x => x.GetY(1).Z.VoidWithIntInt(2, 3));
			}

			[Fact]
			public void Void_method_call_with_matcher()
			{
				AssertReconstructable(
					x => x.VoidWithInt(It.IsAny<int>()),
					x => x.VoidWithInt(It.IsAny<int>()));
			}

			[Fact]
			public void Void_method_call_with_matcher_in_first_of_three_invocations()
			{
				AssertReconstructable(
					x => x.GetY(It.IsAny<int>()).Z.VoidWithIntInt(0, 0),
					x => x.GetY(It.IsAny<int>()).Z.VoidWithIntInt(0, 0));
			}

			[Fact]
			public void Void_method_call_with_matcher_in_third_of_three_invocations_1()
			{
				AssertReconstructable(
					x => x.GetY(0).Z.VoidWithIntInt(1, It.IsAny<int>()),
					x => x.GetY(0).Z.VoidWithIntInt(1, It.IsAny<int>()));
			}

			[Fact]
			public void Void_method_call_with_matcher_in_third_of_three_invocations_2()
			{
				AssertReconstructable(
					x => x.GetY(0).Z.VoidWithIntInt(It.IsAny<int>(), 2),
					x => x.GetY(0).Z.VoidWithIntInt(It.IsAny<int>(), 2));
			}

			[Fact]
			public void Void_method_call_with_matcher_in_first_and_third_of_three_invocations()
			{
				AssertReconstructable(
					"x => x.GetY(It.Is<int>(i => i % 2 == 0)).Z.VoidWithIntInt(It.IsAny<int>(), 2)",
					 x => x.GetY(It.Is<int>(i => i % 2 == 0)).Z.VoidWithIntInt(It.IsAny<int>(), 2));
			}

			[Fact]
			public void Method_with_matchers_after_default_arg()
			{
				// This demonstrates that even though the first argument has a default value,
				// the matcher isn't placed there, because it has a type (string) that won't fit (int).

				AssertReconstructable(
					x => x.VoidWithIntString(0, It.IsAny<string>()),
					x => x.VoidWithIntString(0, It.IsAny<string>()));
			}

			[Fact]
			public void Assignment()
			{
				AssertReconstructable(
					"x => x.GetY().Z.Property = \"value\"",
					 x => x.GetY().Z.Property =  "value" );
			}

			[Fact]
			public void Assignment_with_captured_var_on_rhs()
			{
				var arg = "value";
				AssertReconstructable(
					"x => x.GetY().Z.Property = \"value\"",
					 x => x.GetY().Z.Property = arg);
			}

			[Fact]
			public void Assignment_with_matcher_on_rhs()
			{
				AssertReconstructable(
					"x => x.GetY().Z.Property = It.IsAny<string>()",
					 x => x.GetY().Z.Property = It.IsAny<string>());
			}

			[Fact]
			public void Indexer_assignment_with_arg()
			{
				AssertReconstructable(
					"x => x[1] = null",
					 x => x[1] = null);
			}

			[Fact]
			public void Indexer_assignment_with_matcher_on_lhs_1()
			{
				AssertReconstructable(
					"x => x[It.IsAny<int>()] = null",
					 x => x[It.IsAny<int>()] = null);
			}

			[Fact]
			public void Indexer_assignment_with_matcher_on_lhs_2()
			{
				AssertReconstructable(
					"x => x[1, It.IsAny<int>()] = 0",
					 x => x[1, It.IsAny<int>()] = 0);
			}

			[Fact]
			public void Indexer_assignment_with_matcher_on_rhs()
			{
				AssertReconstructable(
					"x => x[1] = It.IsAny<ActionObserverFixture.Reconstructibility.IY>()",
					 x => x[1] = It.IsAny<ActionObserverFixture.Reconstructibility.IY>());
			}

			[Fact]
			public void Indexer_assignment_with_matchers_everywhere()
			{
				AssertReconstructable(
					"x => x[It.Is<int>(i => i == 0), It.Is<int>(i => i == 2)] = It.Is<int>(i => i == 3)",
					 x => x[It.Is<int>(i => i == 0), It.Is<int>(i => i == 2)] = It.Is<int>(i => i == 3));
			}

			private void AssertReconstructable(string expected, Action<IX> action)
			{
				var expression = ActionObserver.Instance.ReconstructExpression(action);
				Assert.Equal(expected, expression.ToStringFixed());
			}

			private void AssertReconstructable(Expression<Action<IX>> expected, Action<IX> action)
			{
				var actual = ActionObserver.Instance.ReconstructExpression(action);
				Assert.Equal(expected, actual, ExpressionComparer.Default);
			}

			public interface IX
			{
				IY this[int index] { get; set; }
				int this[int index1, int index2] { get; set; }
				IY GetY();
				IY GetY(int arg);
				void Void();
				void VoidWithInt(int arg);
				void VoidWithLong(long arg);
				void VoidWithNullableInt(int? arg);
				void VoidWithIntString(int arg1, string arg2);
			}

			public interface IY
			{
				IZ Z { get; }
			}

			public interface IZ
			{
				object Property { get; set; }
				void Void();
				void VoidWithIntInt(int arg1, int arg2);
			}
		}

		public class Error_detection
		{
			[Fact]
			public void Stops_before_non_interceptable_method()
			{
				AssertFailsAfter<X>("x => x...", x => x.NonVirtual());
			}

			[Fact]
			public void Stops_after_non_interceptable_return_type()
			{
				AssertFailsAfter<IX>("x => x.SealedY...", x => x.SealedY.Method());
			}

			private void AssertFailsAfter<TRoot>(string expectedPartial, Action<TRoot> action)
			{
				var error = Assert.Throws<ArgumentException>(() => ActionObserver.Instance.ReconstructExpression(action));
				Assert.Contains($": {expectedPartial}", error.Message);
			}

			public interface IX
			{
				IY Y { get; }
				SealedY SealedY { get; }
			}

			public class X
			{
				public void NonVirtual() { }
			}

			public interface IY
			{
				void Method(int arg1, int arg2);
			}

			public sealed class SealedY
			{
				public void Method() { }
			}
		}

		// These tests document limitations of the current implementation.
		public class Limitations
		{
			[Fact]
			public void Method_with_matchers_after_default_arg()
			{
				// This is because parameters with default values are filled from left to right.

				AssertIncorrectlyReconstructsAs(
					x => x.Method(It.IsAny<int>(), 0              ),
					x => x.Method(0              , It.IsAny<int>()));
			}

			[Fact]
			public void Indexer_with_default_value_on_lfs_and_matcher_on_rhs_both_having_same_types()
			{
				// Same as above, since LHS and RHS are actually both part of a single parameter list of a method call `get_Item(...lhs, rhs).
				AssertIncorrectlyReconstructsAs(
					"x => x[It.IsAny<int>()] = 0",
					 x => x[0              ] = It.IsAny<int>());
			}

			private void AssertIncorrectlyReconstructsAs(string expected, Action<IX> action)
			{
				var expression = ActionObserver.Instance.ReconstructExpression(action);
				Assert.Equal(expected, expression.ToStringFixed());
			}

			private void AssertIncorrectlyReconstructsAs(Expression<Action<IX>> expected, Action<IX> action)
			{
				var actual = ActionObserver.Instance.ReconstructExpression(action);
				Assert.Equal(expected, actual, ExpressionComparer.Default);
			}

			public interface IX
			{
				int this[int index] { get; set; }
				void Method(int arg1, int arg2);
			}
		}
	}
}
