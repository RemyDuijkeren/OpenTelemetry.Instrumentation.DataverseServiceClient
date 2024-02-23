# Testing

## Test Frameworks and libraries

As test framework we use [xUnit](https://xunit.net/). For mocking we use [NSubstitute](https://nsubstitute.github.io/).

We use [AutoBogus](https://github.com/nickdodd79/AutoBogus), which uses [Bogus](https://github.com/bchavez/Bogus)
to generate fake data. To use AutoBogus, we only need to reference [AutoBogus.NSubstitute](https://www.nuget.org/packages/AutoBogus.NSubstitute),
the NSubtitute binding for AutoBogus.

## Test structure

### Test naming

We follow the Should_ExpectedBehavior_When_StateUnderTest pattern for naming our tests. This pattern is inspired by
the [BDD](https://en.wikipedia.org/wiki/Behavior-driven_development) (Behavior Driven Development) style of writing.

This will result in a test methods name like below, where we prefer to remove the _Should_ prefix, because it's redundant:

    (Should_)[ExpectedBehavior]_When_[PreCondition(s)]

    examples:
    ShouldThrowException_When_AgeLessThan18 // not prefered
    ThrowException_When_AgeLessThan18
    ReturnContact_When_ContactIdExists
    ThrowArgumentException_When_ContactIdIsNullOrWhitespace
    PublishContactCreatedEvent_When_ContactIsCreated
    SendCreateContactCommand_When_ContactIsCreated
    SumTwoNumbers_When_NumbersArePositive
    SumTwoNumbers // omit the When_ part if there are no preconditions or only one precondition

For class names we use the below naming convention. Here the class name is the SUT (System-Under-Test), often the
class under test. Optional the method name is added to the class name to group the tests for the method together.
The group even more we can add the main preconditions to the class name, instead of the method name.
Don`t add the Suffix Test to the class name.

    [TestedClassName](_[TestedMethodName])(_When_[Main PreCondition(s)])

    examples:
    OpenTelemetryServiceClientDecorator
    OpenTelemetryServiceClientDecorator_Create
    OpenTelemetryServiceClientDecorator_Create_WhenServiceClientIsNull

Optional we can group test classes in a folder with the same name as the class under test. For folder names we use the
class name under test with the Suffix Test.

    [TestedClassName]Tests

    examples:
    OpenTelemetryServiceClientDecoratorTests

### Arrange, Act, Assert (AAA)

We follow the Arrange, Act, Assert (AAA) pattern for structuring our tests. This pattern is inspired by the
[AAA](https://en.wikipedia.org/wiki/Arrange-Act-Assert) (Arrange, Act, Assert) pattern.

    Arrange: setup the preconditions for the test
    Act: execute the method under test
    Assert: verify the expected outcome

```c#
    [Fact]
    public void ReturnsContact_WhenContactIdExists()
    {
        // arrange
        var contactId = Guid.NewGuid();
        var contact = new Contact { Id = contactId };
        var contactRepository = Substitute.For<IContactRepository>();
        contactRepository.GetById(contactId).Returns(contact);
        var sut = new ContactService(contactRepository);

        // act
        var result = sut.GetContact(contactId);

        // assert
        Assert.Equal(contact, result);
    }
```

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

### Test behaviour, not implementation

We test the behaviour of the System Under Test (SUT), not the implementation. We don't care how the SUT does it, only
that it does it.

So we don't test private methods, because they are an implementation detail. We test the public methods, because they
are the interface of the SUT. Don't test all the details or in-between steps. Test the expected outcome to prevent
brittle tests.

For more info see:
https://enterprisecraftsmanship.com/posts/stubs-vs-mocks/
https://enterprisecraftsmanship.com/posts/when-to-mock/
