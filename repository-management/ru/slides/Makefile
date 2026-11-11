# Slides for the two repository-management lectures. The theme is borrowed
# from the .NET course and lives in dotnet-course/theme/.

DECKS := RM1-repository-slides RM2-delivery-slides
THEME := ../../../dotnet-course/theme

PDFS  := $(addsuffix .pdf,$(DECKS))
NPDFS := $(addsuffix -notes.pdf,$(DECKS))
STY   := $(THEME)/beamerthemedotnet.sty

# The theme is not in this directory, so tell kpathsea where to look. The
# trailing colon keeps the normal TeX tree on the path.
export TEXINPUTS := .:$(THEME):

LATEXMK := latexmk -pdf -halt-on-error -interaction=nonstopmode

.PHONY: all notes clean

all: $(PDFS)

%.pdf: %.tex $(STY)
	$(LATEXMK) $<

# Deck plus speaker notes on a second screen (right half of each page).
notes: $(NPDFS)

%-notes.pdf: %.tex $(STY)
	$(LATEXMK) -jobname=$*-notes \
	  -pdflatex='pdflatex %O "\def\shownotes{}\input{%S}"' $<

clean:
	latexmk -C 2>/dev/null || true
	rm -f *.aux *.log *.nav *.out *.snm *.toc *.vrb *.fls *.fdb_latexmk *.synctex.gz
	rm -f $(PDFS) $(NPDFS)
