# Repository management course — house rules

Applies to everything under `repository-management/`. Two lectures, laid out
like the .NET course: `ru/course-structure.md` for the tree, and one folder
per lecture under `ru/lectures/`, each with its script (`Lnn-*.md`, the source
of truth), its theses (`README.md`), its own `CLAUDE.md` and its deck
(`slides/*.tex`).

The decks use the .NET course's theme and follow its presentation rules — the
palette, the two-colour system, what may go on a slide, the slide-cue
convention, the build. They are not repeated here: read
[`../dotnet-course/CLAUDE.md`](../dotnet-course/CLAUDE.md) and
[`../dotnet-course/theme/README.md`](../dotnet-course/theme/README.md).

What differs from the .NET course:

- **The theme is not copied.** Each lecture's `Makefile` points at
  `dotnet-course/theme/`.
- **There are no labs** and no `lecture-notes.md` yet.
- **The audience is first-year students.** Git is taught as a model and a
  handful of commands; the weight is on concepts.
- **The sample repository is `HelloWorld`**, and terminal output is real git
  output, as in the .NET course.

```sh
cd ru/lectures/Lnn-*/slides && make        # also: make notes / watch / clean
```
