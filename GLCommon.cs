using System;
using OpenTK;

namespace Fractals
{
	struct Color {
		public float red;
		public float green;
		public float blue;
		public float alpha;
	}

	struct TextureCoord {
		public float S;
		public float T;
	}

	public static class GLCommon
	{
		public static float radiansFromDegrees (float degrees)
		{
			return (float)Math.PI * degrees / 180.0f;
		}
			
		public static void Matrix3DSetIdentity (ref float[] matrix)
		{
			matrix [0] = matrix [5] = matrix [10] = matrix [15] = 1.0f;
			matrix [1] = matrix [2] = matrix [3] = matrix [4] = 0.0f;
			matrix [6] = matrix [7] = matrix [8] = matrix [9] = 0.0f;
			matrix [11] = matrix [12] = matrix [13] = matrix [14] = 0.0f;
		}

		public static void Matrix3DSetTranslation (ref float[] matrix, float xTranslate, float yTranslate, float zTranslate)
		{
			matrix [0] = matrix [5] = matrix [10] = matrix [15] = 1.0f;
			matrix [1] = matrix [2] = matrix [3] = matrix [4] = 0.0f;
			matrix [6] = matrix [7] = matrix [8] = matrix [9] = 0.0f;
			matrix [11] = 0.0f;
			matrix [12] = xTranslate;
			matrix [13] = yTranslate;
			matrix [14] = zTranslate;
		}

		public static void Matrix3DSetScaling (ref float[] matrix, float xScale, float yScale, float zScale)
		{
			matrix [1] = matrix [2] = matrix [3] = matrix [4] = 0.0f;
			matrix [6] = matrix [7] = matrix [8] = matrix [9] = 0.0f;
			matrix [11] = matrix [12] = matrix [13] = matrix [14] = 0.0f;
			matrix [0] = xScale;
			matrix [5] = yScale;
			matrix [10] = zScale;
			matrix [15] = 1.0f;
		}

		public static float[] Matrix3DMultiply (float[] m1, float[] m2)
		{
			float[] result = new float[16];

			result [0] = m1 [0] * m2 [0] + m1 [4] * m2 [1] + m1 [8] * m2 [2] + m1 [12] * m2 [3];
			result [1] = m1 [1] * m2 [0] + m1 [5] * m2 [1] + m1 [9] * m2 [2] + m1 [13] * m2 [3];
			result [2] = m1 [2] * m2 [0] + m1 [6] * m2 [1] + m1 [10] * m2 [2] + m1 [14] * m2 [3];
			result [3] = m1 [3] * m2 [0] + m1 [7] * m2 [1] + m1 [11] * m2 [2] + m1 [15] * m2 [3];

			result [4] = m1 [0] * m2 [4] + m1 [4] * m2 [5] + m1 [8] * m2 [6] + m1 [12] * m2 [7];
			result [5] = m1 [1] * m2 [4] + m1 [5] * m2 [5] + m1 [9] * m2 [6] + m1 [13] * m2 [7];
			result [6] = m1 [2] * m2 [4] + m1 [6] * m2 [5] + m1 [10] * m2 [6] + m1 [14] * m2 [7];
			result [7] = m1 [3] * m2 [4] + m1 [7] * m2 [5] + m1 [11] * m2 [6] + m1 [15] * m2 [7];

			result [8] = m1 [0] * m2 [8] + m1 [4] * m2 [9] + m1 [8] * m2 [10] + m1 [12] * m2 [11];
			result [9] = m1 [1] * m2 [8] + m1 [5] * m2 [9] + m1 [9] * m2 [10] + m1 [13] * m2 [11];
			result [10] = m1 [2] * m2 [8] + m1 [6] * m2 [9] + m1 [10] * m2 [10] + m1 [14] * m2 [11];
			result [11] = m1 [3] * m2 [8] + m1 [7] * m2 [9] + m1 [11] * m2 [10] + m1 [15] * m2 [11];

			result [12] = m1 [0] * m2 [12] + m1 [4] * m2 [13] + m1 [8] * m2 [14] + m1 [12] * m2 [15];
			result [13] = m1 [1] * m2 [12] + m1 [5] * m2 [13] + m1 [9] * m2 [14] + m1 [13] * m2 [15];
			result [14] = m1 [2] * m2 [12] + m1 [6] * m2 [13] + m1 [10] * m2 [14] + m1 [14] * m2 [15];
			result [15] = m1 [3] * m2 [12] + m1 [7] * m2 [13] + m1 [11] * m2 [14] + m1 [15] * m2 [15];

			return result;
		}

		public static float[] MatrixVectorMultiply(float[] m, float[] v)
		{
			float[] result = new float[4];

			result [0] = m [0] * v [0] + m [4] * v [1] + m [8] * v [2] + m [12] * v [3];
			result [1] = m [1] * v [0] + m [5] * v [1] + m [9] * v [2] + m [13] * v [3];
			result [2] = m [2] * v [0] + m [6] * v [1] + m [10] * v [2] + m [14] * v [3];
			result [3] = m [3] * v [0] + m [7] * v [1] + m [11] * v [2] + m [15] * v [3];

			return result;
		}
	}
}