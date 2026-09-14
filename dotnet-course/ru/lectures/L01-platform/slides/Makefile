# Слайды Л1. Тема общая для всего курса и лежит в dotnet-course/theme/.

DECK  := L01-platform-slides
THEME := ../../../../theme

PDF  := $(DECK).pdf
NPDF := $(DECK)-notes.pdf
STY  := $(THEME)/beamerthemedotnet.sty

# Тема лежит не в этом каталоге — показываем kpathsea, где искать.
# Хвостовое двоеточие сохраняет обычное дерево TeX в пути.
export TEXINPUTS := .:$(THEME):

LATEXMK := latexmk -pdf -halt-on-error -interaction=nonstopmode

.PHONY: all notes watch clean

all: $(PDF)

$(PDF): $(DECK).tex $(STY)
	$(LATEXMK) $(DECK).tex

# Колода плюс заметки докладчика на втором экране (правая половина страницы).
notes: $(DECK).tex $(STY)
	$(LATEXMK) -jobname=$(NPDF:.pdf=) \
	  -pdflatex='pdflatex %O "\def\shownotes{}\input{%S}"' $(DECK).tex

watch:
	latexmk -pdf -pvc $(DECK).tex

clean:
	latexmk -C 2>/dev/null || true
	rm -f *.aux *.log *.nav *.out *.snm *.toc *.vrb *.fls *.fdb_latexmk *.synctex.gz
	rm -f $(PDF) $(NPDF)
