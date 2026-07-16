# Pondhawk.Rql

A Resource Query Language (RQL) implementation with AST, fluent builder, text parser, and serialization to LINQ lambdas, SQL, RQL text, and English descriptions. Fully standalone — no dependency on any other Pondhawk package.

> API documentation and usage examples live in [`src/Pondhawk.Rql/README.md`](src/Pondhawk.Rql/README.md) (which is also the NuGet package readme).

## Repository Layout

```
src/Pondhawk.Rql/         The library (net8.0)
test/Pondhawk.Rql.Tests/  xUnit tests
build/                    Cake (Frosting) build script
.github/workflows/        CI (build/pack/push) + NuGet.org publish
```

## Building

```bash
# Restore, build, and test
dotnet run --project build/Build.csproj -- --target=Test

# Or use the SDK directly
dotnet build pondhawk-rql.slnx
dotnet test pondhawk-rql.slnx
```

## Versioning & Packaging

The package version is `major.minor` from [`src/Pondhawk.Rql/version.json`](src/Pondhawk.Rql/version.json), with the CI build number appended. Off-CI builds get a `-local` prerelease suffix.

```bash
# Produce a .nupkg in ./artifacts
dotnet run --project build/Build.csproj -- --target=Pack --build-number=<n>
```

## CI/CD

- **`build.yml`** — on push/PR to `main`: build + test; on `main` it also packs and pushes to the pondhawk GitHub Packages feed and uploads the `.nupkg` as an artifact.
- **`publish.yml`** — manual `workflow_dispatch` that promotes a build's artifacts to **NuGet.org**. Requires a `NUGET_ORG_API_KEY` repository secret.

## History

Extracted from the [pondhawktech/tools](https://github.com/pondhawktech/tools) monorepo, where it had no internal dependents.

## License

MIT — see [LICENSE](LICENSE).
