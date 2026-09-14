# Slides for Л1. The theme is shared by the whole course and lives in
# dotnet-course/theme/.

DECK  := L01-platform-slides
THEME := ../../../../theme

PDF  := $(DECK).pdf
NPDF := $(DECK)-notes.pdf
STY  := $(THEME)/beamerthemedotnet.sty

# The theme is not in this directory, so tell kpathsea where to look. The
# trailing colon keeps the normal TeX tree on the path.
export TEXINPUTS := .:$(THEME):

LATEXMK := latexmk -pdf -halt-on-error -interaction=nonstopmode

.PHONY: all notes watch clean

all: $(PDF)

$(PDF): $(DECK).tex $(STY)
	$(LATEXMK) $(DECK).tex

# Deck plus speaker notes on a second screen (right half of each page).
notes: $(DECK).tex $(STY)
	$(LATEXMK) -jobname=$(NPDF:.pdf=) \
	  -pdflatex='pdflatex %O "\def\shownotes{}\input{%S}"' $(DECK).tex

watch:
	latexmk -pdf -pvc $(DECK).tex

clean:
	latexmk -C 2>/dev/null || true
	rm -f *.aux *.log *.nav *.out *.snm *.toc *.vrb *.fls *.fdb_latexmk *.synctex.gz
	rm -f $(PDF) $(NPDF)
