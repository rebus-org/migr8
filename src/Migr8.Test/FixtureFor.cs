using System;
using NUnit.Framework;

namespace Migr8.Test
{
    public abstract class FixtureFor<TSut>
    {
        protected TSut sut;

        [SetUp]
        public void RunSetUp()
        {
            sut = SetUp();
        }

        protected abstract TSut SetUp();

        [TearDown]
        public void RunTearDown()
        {
            try
            {
                Console.WriteLine("Tearing down");
                TearDown();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                var disposable = sut as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        protected virtual void TearDown() { }
    }
}