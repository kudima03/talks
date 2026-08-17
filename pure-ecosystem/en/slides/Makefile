TEX := pure-ecosystem-slides.tex
PDF := pure-ecosystem-slides.pdf
NOTES := pure-ecosystem-slides-notes.pdf

all: $(PDF)

$(PDF): $(TEX)
	latexmk -pdf -halt-on-error -interaction=nonstopmode $<

$(NOTES): $(TEX)
	latexmk -pdf -halt-on-error -interaction=nonstopmode \
	  -jobname=$(@:.pdf=) \
	  -pdflatex='pdflatex %O "\def\shownotes{}\input{%S}"' $<

notes: $(NOTES)
