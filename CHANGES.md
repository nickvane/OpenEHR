# Changes #
## Changes in openEHR specification implementation ##

Currently none.

## Changes in software implementation ##

- Upgrade to .net framework 4.5
- Make use of new types in BCL: Lazy<T> (instead of own implementation)
- Remove unused references (Windows.Forms)
- Remove dependency on Microsoft Enterprise Library
	- ObjectBuilder (dependency injection): this should not be part of the reference model, but should be in the application that uses the model. A better supported DI framework should be used (like Autofac, ...)
	- Configuration: An implementation of retrieving terminology is provided, but this should not be part of the reference model.

