# Shared build rules for every lecture deck in the .NET course.
#
# A lecture's own Makefile is three lines: the deck basename, where the theme
# lives, and an include of this file. Everything else -- the engine, the notes
# build, the clean list -- is defined once, here, so lecture 12 builds exactly
# the way lecture 1 does.
#
#   DECK  := L01-platform-slides
#   THEME := ../../../../theme
#   include $(THEME)/dotnet-slides.mk

PDF   := $(DECK).pdf
NPDF  := $(DECK)-notes.pdf
STY   := $(THEME)/beamerthemedotnet.sty

# The theme is not in this directory, so tell kpathsea where to look. The
# trailing colon keeps the normal TeX tree on the path.
export TEXINPUTS := .:$(THEME):

LATEXMK := latexmk -pdf -halt-on-error -interaction=nonstopmode

.PHONY: all notes watch clean proof

all: $(PDF)

$(PDF): $(DECK).tex $(STY)
	$(LATEXMK) $(DECK).tex

# Deck plus speaker notes on a second screen (right half of each page).
notes: $(DECK).tex $(STY)
	$(LATEXMK) -jobname=$(DECK)-notes \
	  -pdflatex='pdflatex %O "\def\shownotes{}\input{%S}"' $(DECK).tex

watch:
	latexmk -pdf -pvc $(DECK).tex

# Look at the pages, not just the log: bad wraps and collisions produce no
# warning. Needs poppler-utils; the montage step additionally needs
# imagemagick and is skipped when it is missing.
proof: $(PDF)
	@mkdir -p .proof && rm -f .proof/p-*.png
	@pdftoppm -r 45 -png $(PDF) .proof/p
	@command -v montage >/dev/null \
	  && montage .proof/p-*.png -tile 5x -geometry +4+4 .proof/contact-sheet.png \
	  && echo "contact sheet: .proof/contact-sheet.png" \
	  || echo "pages in .proof/ (install imagemagick for a contact sheet)"

clean:
	latexmk -C 2>/dev/null || true
	rm -f *.aux *.log *.nav *.out *.snm *.toc *.vrb *.fls *.fdb_latexmk *.synctex.gz
	rm -f $(PDF) $(NPDF)
	rm -rf .proof
