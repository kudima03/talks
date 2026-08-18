TEX  := pure-ecosystem-slides.tex
THEME := beamerthemepure.sty
PDF  := pure-ecosystem-slides.pdf
NPDF := pure-ecosystem-slides-notes.pdf

.PHONY: all notes clean watch

all: $(PDF)

$(PDF): $(TEX) $(THEME)
	latexmk -pdf -halt-on-error -interaction=nonstopmode $(TEX)

# Deck plus speaker notes on a second screen (right half of each page).
notes:
	latexmk -pdf -halt-on-error -interaction=nonstopmode \
	  -jobname=pure-ecosystem-slides-notes \
	  -pdflatex='pdflatex %O "\def\shownotes{}\input{%S}"' $(TEX)

watch:
	latexmk -pdf -pvc $(TEX)

clean:
	latexmk -C
	rm -f *.aux *.log *.nav *.out *.snm *.toc *.vrb *.fls *.fdb_latexmk *.synctex.gz
	rm -f $(PDF) $(NPDF)
