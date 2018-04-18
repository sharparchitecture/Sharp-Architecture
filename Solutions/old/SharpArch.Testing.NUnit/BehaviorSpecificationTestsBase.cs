namespace SharpArch.Testing.NUnit
{
    using System;
    using global::NUnit.Framework;
    using JetBrains.Annotations;

    /// <summary>
    ///     Provides a base class for BDD unit tests, as described at http://flux88.com/blog/the-transition-from-tdd-to-bdd/.
    ///     This is an optional base class which need not be used with unit tests.
    /// </summary>
    [PublicAPI]
    public abstract class BehaviorSpecificationTestsBase
    {
        /// <summary>
        ///     Returns an exception thrown during execution of the <see cref="Act" /> method.
        /// </summary>
        [CanBeNull]
        protected Exception ExceptionThrown { get; private set; }

        /// <summary>
        ///     Perform actions upon the model.  Separate test methods would then be employed to verify
        ///     the results of the system under test.
        /// </summary>
        protected abstract void Act();

        /// <summary>
        ///     Method used to setup the model that will be tested.  This includes creating mock objects,
        ///     preparing the model properties, preparing the database, etc.
        /// </summary>
        protected abstract void EstablishContext();

        /// <summary>
        ///     Establishes test context and runs the <see cref="Act" /> method.
        ///     Execetuion exception will be captured in <see cref="ExceptionThrown" /> property.
        /// </summary>
        [SetUp]
        protected virtual void SetUp()
        {
            this.EstablishContext();

            try
            {
                this.Act();
            }
            catch (Exception exc)
            {
                this.ExceptionThrown = exc;
            }
        }
    }
}
