namespace Errable.Tests;

public class CodeTests
{
    [Fact]
    public void Code_StringConstructor_ShouldStoreValue()
    {
        // Arrange
        const string expectedCode = "ERROR_CODE";

        // Act
        var code = new Code(expectedCode);

        // Assert
        Assert.Equal(expectedCode, code.ToString());
    }

    [Fact]
    public void Code_IntConstructor_ShouldStoreValue()
    {
        // Arrange
        const int expectedCode = 404;

        // Act
        var code = new Code(expectedCode);

        // Assert
        Assert.Equal(expectedCode.ToString(), code.ToString());
    }

    [Fact]
    public void Code_ImplicitString_ShouldWork()
    {
        // Arrange
        const string expectedCode = "IMPLICIT_STRING";

        // Act
        Code code = expectedCode;

        // Assert
        Assert.Equal(expectedCode, code.ToString());
    }

    [Fact]
    public void Code_ImplicitInt_ShouldWork()
    {
        // Arrange
        const int expectedCode = 500;

        // Act
        Code code = expectedCode;

        // Assert
        Assert.Equal(expectedCode.ToString(), code.ToString());
    }

    [Fact]
    public void Code_Equality_SameString_ShouldBeEqual()
    {
        // Arrange
        Code code1 = "SAME_CODE";
        Code code2 = "SAME_CODE";

        // Act & Assert
        Assert.True(code1 == code2);
        Assert.True(code1.Equals(code2));
        Assert.False(code1 != code2);
    }

    [Fact]
    public void Code_Equality_SameInt_ShouldBeEqual()
    {
        // Arrange
        Code code1 = 123;
        Code code2 = 123;

        // Act & Assert
        Assert.True(code1 == code2);
        Assert.True(code1.Equals(code2));
        Assert.False(code1 != code2);
    }

    [Fact]
    public void Code_Equality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        Code code1 = "CODE1";
        Code code2 = "CODE2";

        // Act & Assert
        Assert.False(code1 == code2);
        Assert.False(code1.Equals(code2));
        Assert.True(code1 != code2);
    }

    [Fact]
    public void Code_Equality_StringVsInt_ShouldNotBeEqual()
    {
        // Arrange
        Code stringCode = "123";
        Code intCode = 123;

        // Act & Assert
        Assert.False(stringCode == intCode);
        Assert.False(stringCode.Equals(intCode));
        Assert.True(stringCode != intCode);
    }

    [Fact]
    public void Code_GetHashCode_SameValues_ShouldReturnSameHash()
    {
        // Arrange
        Code code1 = "HASH_TEST";
        Code code2 = "HASH_TEST";

        // Act & Assert
        Assert.Equal(code1.GetHashCode(), code2.GetHashCode());
    }

    [Fact]
    public void Code_StringConstructor_NullValue_ShouldUseEmptyString()
    {
        // Act
        var code = new Code((string)null!);

        // Assert
        Assert.Equal(string.Empty, code.ToString());
    }

    [Fact]
    public void Code_ToString_ShouldReturnStringRepresentation()
    {
        // Arrange
        var stringCode = new Code("STRING_CODE");
        var intCode = new Code(42);

        // Act & Assert
        Assert.Equal("STRING_CODE", stringCode.ToString());
        Assert.Equal("42", intCode.ToString());
    }
}