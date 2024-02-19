# Testing

## Test Frameworks and libraries

As test framework we use [xUnit](https://xunit.net/). For mocking we use [NSubstitute](https://nsubstitute.github.io/).

We use [AutoBogus](https://github.com/nickdodd79/AutoBogus), which uses [Bogus](https://github.com/bchavez/Bogus)
to generate fake data. To use AutoBogus, we only need to reference [AutoBogus.NSubstitute](https://www.nuget.org/packages/AutoBogus.NSubstitute),
the NSubtitute binding for AutoBogus.

## Guidelines

### Test naming (Given-When-Then)

Write the class as the SUT (system under test) or the action/unit for the behavior (to express the _When_) without
Tests at the end.

    [ClassName/Action/Unit/Behavior/When](_Given[Main PreCondition(s)])

The methods have the pattern (to express the _Should_):

* **Returns**[ExpectedOutput]**_Given**[PreCondition(s)]
* **Throw**[ExpectedException]**_Given**[PreCondition(s)]
* **Publish**[ExpectedEvent]**_Given**[PreCondition(s)]
* **Send**[ExpectedCommand]**_Given**[PreCondition(s)]

This looks like:

    public class FindContact : BaseTest
    {
        [Fact]
        public void ReturnsContact_GivenContactIdExists()
        {
            // ...
        }

        [Fact]
        public void ThrowArgumentException_GivenContactIdIsNullOrWhitespace()
        {
            // ...
        }
    }

### Stub vs Mock

**Stubs** are used for querying (=reading) external dependencies that we don't control. We don't care about verifying
the calls, because it's input for the System Under Test (SUT). We are only interested in the outcome of the test.

Using [NSubstitue](https://nsubstitute.github.io/) (as our mocking framework) we can create a stub like so:

```c#
    // arrange
    var someThingStub = Substitute.For<ISomeThing>();
    someThingStub.Execute(Arg.Any<string>).Returns("Hello world!");

    // act

    // assert
    // We don't assert a stub!
```
If possible try to not use stubs, but just call the real dependency, so that we know early when the dependency is
changed.

**Mocks** are used for commanding (changing) external dependencies that we don't control (unmanaged). We want to verify
that our command happened, because this is the output of our System Under Test (SUT).

```c#
    // arrange
    var someThingMock = Substitute.For<ISomeThing>();
    someThingMock.Execute(Arg.Any<string>).Returns("Hello world!");

    // act

    // assert
    someThingMock.Recieved().Execute(); // this will verify that the mock has be called
```

White paper testing with mocks: https://www.jamesshore.com/v2/projects/testing-without-mocks/testing-without-mocks

### Test the outgoing behaviour of your SUT (system under test)

Don't test all the details or in-between steps. Test the expected outcome to prevent brittle tests.

For more info see:
https://enterprisecraftsmanship.com/posts/stubs-vs-mocks/
https://enterprisecraftsmanship.com/posts/when-to-mock/
