import raquel as raquel
from itertools import imap,ifilter
print raquel.fetch("({handle:'wall'})-[:`HAS_MATERIAL`]->(b)")

paths = raquel.fetch("({handle:'standing_human'})-[e*1..3]->({handle:'phone'})")
print paths
print raquel.SortBy(paths,'Belief')